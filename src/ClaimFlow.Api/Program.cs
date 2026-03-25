using ClaimFlow.Api.Extensions;
using ClaimFlow.Api.Middlewares;
using ClaimFlow.Application.Behaviors;
using ClaimFlow.Application.Features.Tenants.Commands.CreateTenant;
using ClaimFlow.Application.Interfaces;
using ClaimFlow.Infrastructure.Consumers;
using ClaimFlow.Infrastructure.Data;
using ClaimFlow.Infrastructure.Jobs;
using ClaimFlow.Infrastructure.Services;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

// Bootstrap Serilog early so startup errors are logged
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.WithProperty("Application", "ClaimFlow")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // CORS for React frontend
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // MediatR
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(CreateTenantCommand).Assembly));

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.UseVector());
    });
    builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

    // Validation
    builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantCommand).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Outbox processor
    builder.Services.AddHostedService<ClaimFlow.Infrastructure.BackgroundServices.OutboxProcessor>();

    // MassTransit + RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(typeof(ClaimSubmittedConsumer).Assembly);

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitConfig = builder.Configuration.GetSection("RabbitMq");
            cfg.Host(rabbitConfig["Host"] ?? "localhost", "/", h =>
            {
                h.Username(rabbitConfig["Username"] ?? "claimflow");
                h.Password(rabbitConfig["Password"] ?? "claimflow_dev_2026");
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    // Fraud detection
    builder.Services.AddScoped<IEmbeddingService, FakeEmbeddingService>();
    builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();

    // Reporting
    builder.Services.AddScoped<IReportingService, ReportingService>();

    // Hangfire
    builder.Services.AddHangfire(config =>
        config.UsePostgreSqlStorage(c =>
            c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")!)));
    builder.Services.AddHangfireServer();

    builder.Services.AddScoped<PremiumRenewalReminderJob>();
    builder.Services.AddScoped<StaleClaimAlertJob>();
    builder.Services.AddScoped<PolicyExpirationJob>();
    builder.Services.AddScoped<MonthlyStatementJob>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: new[] { "db", "ready" });

    // OpenTelemetry tracing
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("ClaimFlow.Api"))
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("MassTransit")
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                });
        });


    var app = builder.Build();

    // Serilog request logging (replaces default Microsoft request logging)
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();


    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseHangfireDashboard("/hangfire");

    // Health check endpoints
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // liveness = just "is the app running?"
    });

    // API endpoints
    app.MapGet("/api/health", () => Results.Ok("Api is running"));
    app.MapTenantEndpoint();
    app.MapCustomerEndpoint();
    app.MapPolicyEndpoint();
    app.MapClaimEndpoint();
    app.MapReportingEndpoints();

    // Hangfire recurring jobs
    RecurringJob.AddOrUpdate<PremiumRenewalReminderJob>(
        "premium-renewal-reminder",
        job => job.ExecuteAsync(),
        Cron.Daily(9, 0));

    RecurringJob.AddOrUpdate<StaleClaimAlertJob>(
        "stale-claim-alert",
        job => job.ExecuteAsync(),
        Cron.Daily(8, 0));

    RecurringJob.AddOrUpdate<PolicyExpirationJob>(
        "policy-expiration",
        job => job.ExecuteAsync(),
        Cron.Daily(0, 0));

    RecurringJob.AddOrUpdate<MonthlyStatementJob>(
        "monthly-statement",
        job => job.ExecuteAsync(),
        Cron.Monthly(1, 6, 0));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
