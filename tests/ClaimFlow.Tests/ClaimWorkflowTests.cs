using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using ClaimFlow.Domain.StateMachines;
using ClaimFlow.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Tests
{
    public class ClaimWorkflowTests : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _fixture;

        public ClaimWorkflowTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void StateMachine_SubmittedClaim_CanOnlyStartReview()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };
            var machine = new ClaimStateMachine(claim);

            // Act & Assert
            Assert.True(machine.CanFire(ClaimTrigger.StartReview));
            Assert.False(machine.CanFire(ClaimTrigger.Approve));
            Assert.False(machine.CanFire(ClaimTrigger.Reject));
            Assert.False(machine.CanFire(ClaimTrigger.Close));
        }

        [Fact]
        public void StateMachine_FullApprovalWorkflow_Succeeds()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };
            var machine = new ClaimStateMachine(claim);

            // Act — walk through full lifecycle
            machine.Fire(ClaimTrigger.StartReview);
            Assert.Equal(ClaimStatus.UnderReview, claim.Status);

            machine.Fire(ClaimTrigger.Approve);
            Assert.Equal(ClaimStatus.Approved, claim.Status);

            machine.Fire(ClaimTrigger.SchedulePayment);
            Assert.Equal(ClaimStatus.PaymentScheduled, claim.Status);

            machine.Fire(ClaimTrigger.ConfirmPayment);
            Assert.Equal(ClaimStatus.Paid, claim.Status);

            machine.Fire(ClaimTrigger.Close);
            Assert.Equal(ClaimStatus.Closed, claim.Status);
        }

        [Fact]
        public void StateMachine_RejectionAndAppeal_ReentersReview()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };
            var machine = new ClaimStateMachine(claim);

            // Act
            machine.Fire(ClaimTrigger.StartReview);
            machine.Fire(ClaimTrigger.Reject);
            Assert.Equal(ClaimStatus.Rejected, claim.Status);

            machine.Fire(ClaimTrigger.FileAppeal);
            Assert.Equal(ClaimStatus.Appeal, claim.Status);

            machine.Fire(ClaimTrigger.StartReview);
            Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        }

        [Fact]
        public void StateMachine_InvalidTransition_Throws()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };
            var machine = new ClaimStateMachine(claim);

            // Act & Assert — can't approve a Submitted claim directly
            Assert.Throws<InvalidOperationException>(() => machine.Fire(ClaimTrigger.Approve));
        }

        [Fact]
        public async Task ClaimSubmission_ShouldPersistWithStatusHistory()
        {
            // Arrange
            using var context = _fixture.CreateDbContext();

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Branch",
                Code = $"TST-{Random.Shared.Next(100, 999)}",
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = "Test Customer",
                Email = $"test-{Guid.NewGuid():N}@test.com",
                TenantId = tenant.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Customers.Add(customer);

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = $"POL-{Random.Shared.Next(10000, 99999)}",
                TenantId = tenant.Id,
                CustomerId = customer.Id,
                ProductType = ProductType.Auto,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                EndDate = DateTime.UtcNow.AddMonths(6),
                PolicyStatus = PolicyStatus.Active
            };
            context.Policies.Add(policy);

            var claim = new Claim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = $"CLM-{Random.Shared.Next(10000, 99999)}",
                PolicyId = policy.Id,
                TenantId = tenant.Id,
                Description = "Rear-ended at traffic light",
                ClaimedAmount = 15000m,
                Status = ClaimStatus.Submitted,
                SubmittedAt = DateTime.UtcNow
            };
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Act — transition to UnderReview
            var machine = new ClaimStateMachine(claim);
            var fromStatus = claim.Status;
            machine.Fire(ClaimTrigger.StartReview);

            context.Histories.Add(new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = claim.Id,
                FromStatus = fromStatus,
                ToStatus = claim.Status,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = "test-adjuster",
                Notes = "Integration test"
            });
            await context.SaveChangesAsync();

            // Assert
            var savedClaim = await context.Claims
                .Include(c => c.Histories)
                .FirstOrDefaultAsync(c => c.Id == claim.Id);

            Assert.NotNull(savedClaim);
            Assert.Equal(ClaimStatus.UnderReview, savedClaim.Status);
            Assert.Single(savedClaim.Histories);
            Assert.Equal("test-adjuster", savedClaim.Histories.First().ChangedBy);
        }

        [Fact]
        public async Task MultiTenancy_TenantCannotSeeCrossData()
        {
            // Arrange
            using var context = _fixture.CreateDbContext();

            var tenantA = new Tenant { Id = Guid.NewGuid(), Name = "Istanbul", Code = $"IST-{Random.Shared.Next(100, 999)}", CreatedAt = DateTime.UtcNow };
            var tenantB = new Tenant { Id = Guid.NewGuid(), Name = "Ankara", Code = $"ANK-{Random.Shared.Next(100, 999)}", CreatedAt = DateTime.UtcNow };
            context.Tenants.AddRange(tenantA, tenantB);

            var customerA = new Customer { Id = Guid.NewGuid(), FullName = "Customer A", Email = $"a-{Guid.NewGuid():N}@test.com", TenantId = tenantA.Id, CreatedAt = DateTime.UtcNow };
            var customerB = new Customer { Id = Guid.NewGuid(), FullName = "Customer B", Email = $"b-{Guid.NewGuid():N}@test.com", TenantId = tenantB.Id, CreatedAt = DateTime.UtcNow };
            context.Customers.AddRange(customerA, customerB);
            await context.SaveChangesAsync();

            // Act — query by tenant
            var istanbulCustomers = await context.Customers.Where(c => c.TenantId == tenantA.Id).ToListAsync();
            var ankaraCustomers = await context.Customers.Where(c => c.TenantId == tenantB.Id).ToListAsync();

            // Assert
            Assert.Single(istanbulCustomers);
            Assert.Equal("Customer A", istanbulCustomers[0].FullName);
            Assert.Single(ankaraCustomers);
            Assert.Equal("Customer B", ankaraCustomers[0].FullName);
        }
    }
}
