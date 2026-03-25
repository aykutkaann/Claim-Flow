using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClaimFlow.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job: finds policies expiring within 30 days
    /// and logs reminders (in production, this would send emails/SMS).
    /// </summary>
    public class PremiumRenewalReminderJob
    {
        private readonly string _connectionString;
        private readonly ILogger<PremiumRenewalReminderJob> _logger;

        public PremiumRenewalReminderJob(IConfiguration configuration, ILogger<PremiumRenewalReminderJob> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var expiringPolicies = await connection.QueryAsync<ExpiringPolicyRow>(
                """
                SELECT
                    p."Id" AS PolicyId,
                    p."PolicyNumber",
                    p."EndDate",
                    c."FullName" AS CustomerName,
                    c."Email" AS CustomerEmail
                FROM policies p
                INNER JOIN customers c ON p."CustomerId" = c."Id"
                WHERE p."PolicyStatus" = 0
                AND p."EndDate" BETWEEN NOW() AND NOW() + INTERVAL '30 days'
                ORDER BY p."EndDate"
                """);

            var count = 0;
            foreach (var policy in expiringPolicies)
            {
                _logger.LogInformation(
                    "Renewal reminder: Policy {PolicyNumber} for {CustomerName} ({Email}) expires on {EndDate:yyyy-MM-dd}",
                    policy.PolicyNumber, policy.CustomerName, policy.CustomerEmail, policy.EndDate);
                count++;
            }

            _logger.LogInformation("Premium renewal reminder job completed. {Count} reminders sent.", count);
        }

        private class ExpiringPolicyRow
        {
            public Guid PolicyId { get; set; }
            public string PolicyNumber { get; set; } = "";
            public DateTime EndDate { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerEmail { get; set; } = "";
        }
    }
}
