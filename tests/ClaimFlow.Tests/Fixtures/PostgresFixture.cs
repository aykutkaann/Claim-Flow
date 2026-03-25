using ClaimFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ClaimFlow.Tests.Fixtures
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg16")
            .WithDatabase("claimflow_test")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        public string ConnectionString => _container.GetConnectionString();

        public AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString, npgsql => npgsql.UseVector())
                .Options;

            return new AppDbContext(options);
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            // Apply migrations
            using var context = CreateDbContext();
            await context.Database.MigrateAsync();

            // Enable pgvector extension
            await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector;");
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}
