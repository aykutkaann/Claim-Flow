using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClaimFlow.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job: flags claims sitting in "UnderReview" for more than 7 days.
    /// </summary>
    public class StaleClaimAlertJob
    {
        private readonly string _connectionString;
        private readonly ILogger<StaleClaimAlertJob> _logger;

        public StaleClaimAlertJob(IConfiguration configuration, ILogger<StaleClaimAlertJob> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var staleClaims = await connection.QueryAsync<StaleClaimRow>(
                """
                SELECT
                    c."Id" AS ClaimId,
                    c."ClaimNumber",
                    c."Status",
                    c."SubmittedAt",
                    EXTRACT(DAY FROM NOW() - c."SubmittedAt") AS DaysStale
                FROM claims c
                WHERE c."Status" IN ('UnderReview', 'DocumentsRequested', 'UnderInvestigation')
                AND c."SubmittedAt" < NOW() - INTERVAL '7 days'
                ORDER BY c."SubmittedAt"
                """);

            var count = 0;
            foreach (var claim in staleClaims)
            {
                _logger.LogWarning(
                    "Stale claim alert: {ClaimNumber} has been in '{Status}' for {Days:F0} days.",
                    claim.ClaimNumber, claim.Status, claim.DaysStale);
                count++;
            }

            _logger.LogInformation("Stale claim alert job completed. {Count} stale claims found.", count);
        }

        private class StaleClaimRow
        {
            public Guid ClaimId { get; set; }
            public string ClaimNumber { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime SubmittedAt { get; set; }
            public double DaysStale { get; set; }
        }
    }
}
