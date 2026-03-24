using ClaimFlow.Api.Extensions;
using ClaimFlow.Api.Middlewares;
using ClaimFlow.Application.Behaviors;
using ClaimFlow.Application.Features.Tenants.Commands.CreateTenant;
using ClaimFlow.Application.Interfaces;
using ClaimFlow.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//MediatR services
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTenantCommand).Assembly));


//Db context services
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Prodiver services
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());


//Validation services
builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


//OutBoxProcessor
builder.Services.AddHostedService<ClaimFlow.Infrastructure.BackgroundServices.OutboxProcessor>();






var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();



//End points
app.MapGet("/api/health", () =>
{
    return Results.Ok("Api is running");
});


app.MapTenantEndpoint();

app.MapCustomerEndpoint();

app.MapPolicyEndpoint();

app.MapClaimEndpoint();


app.Run();

