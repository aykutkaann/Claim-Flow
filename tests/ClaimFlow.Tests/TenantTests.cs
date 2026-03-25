using ClaimFlow.Domain.Entities;
using ClaimFlow.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Tests
{
    public class TenantTests : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _fixture;

        public TenantTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateTenant_ShouldPersistToDatabase()
        {
            // Arrange
            using var context = _fixture.CreateDbContext();
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Istanbul Branch",
                Code = "IST-01",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.Tenants.FirstOrDefaultAsync(t => t.Id == tenant.Id);
            Assert.NotNull(saved);
            Assert.Equal("Istanbul Branch", saved.Name);
            Assert.Equal("IST-01", saved.Code);
        }

        [Fact]
        public async Task CreateTenant_DuplicateCode_ShouldFail()
        {
            // Arrange
            using var context = _fixture.CreateDbContext();
            var tenant1 = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Ankara Branch",
                Code = "ANK-01",
                CreatedAt = DateTime.UtcNow
            };
            var tenant2 = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Ankara Branch 2",
                Code = "ANK-01", // duplicate
                CreatedAt = DateTime.UtcNow
            };

            context.Tenants.Add(tenant1);
            await context.SaveChangesAsync();

            // Act & Assert
            context.Tenants.Add(tenant2);
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }
}
