using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClaimFlow.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job: marks policies as Expired when their EndDate has passed.
    /// This is the background job we discussed — PolicyStatus.Expired is not triggered by a user.
    /// </summary>
    public class PolicyExpirationJob
    {
        private readonly string _connectionString;
        private readonly ILogger<PolicyExpirationJob> _logger;

        public PolicyExpirationJob(IConfiguration configuration, ILogger<PolicyExpirationJob> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(
                """
                UPDATE policies
                SET "PolicyStatus" = 1
                WHERE "PolicyStatus" = 0
                AND "EndDate" < NOW()
                """);

            _logger.LogInformation("Policy expiration job completed. {Count} policies marked as expired.", affected);
        }
    }
}
