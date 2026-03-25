using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClaimFlow.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job: generates monthly statements for policyholders.
    /// In production, this would create PDF files and email them.
    /// </summary>
    public class MonthlyStatementJob
    {
        private readonly string _connectionString;
        private readonly ILogger<MonthlyStatementJob> _logger;

        public MonthlyStatementJob(IConfiguration configuration, ILogger<MonthlyStatementJob> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var statements = await connection.QueryAsync<StatementRow>(
                """
                SELECT
                    c."Id" AS CustomerId,
                    c."FullName",
                    c."Email",
                    COUNT(DISTINCT p."Id") AS ActivePolicies,
                    COALESCE(SUM(pr."Amount") FILTER (WHERE pr."PremiumStatus" = 1), 0) AS TotalPaidPremiums,
                    COALESCE(SUM(pr."Amount") FILTER (WHERE pr."PremiumStatus" = 0), 0) AS TotalPendingPremiums,
                    COUNT(DISTINCT cl."Id") AS TotalClaims
                FROM customers c
                INNER JOIN policies p ON c."Id" = p."CustomerId" AND p."PolicyStatus" = 0
                LEFT JOIN premiums pr ON p."Id" = pr."PolicyId"
                LEFT JOIN claims cl ON p."Id" = cl."PolicyId"
                    AND cl."SubmittedAt" >= date_trunc('month', NOW()) - INTERVAL '1 month'
                    AND cl."SubmittedAt" < date_trunc('month', NOW())
                GROUP BY c."Id", c."FullName", c."Email"
                HAVING COUNT(DISTINCT p."Id") > 0
                ORDER BY c."FullName"
                """);

            var count = 0;
            foreach (var stmt in statements)
            {
                _logger.LogInformation(
                    "Monthly statement for {Name} ({Email}): {Policies} active policies, " +
                    "{PaidPremiums:C} paid, {PendingPremiums:C} pending, {Claims} claims last month.",
                    stmt.FullName, stmt.Email, stmt.ActivePolicies,
                    stmt.TotalPaidPremiums, stmt.TotalPendingPremiums, stmt.TotalClaims);
                count++;
            }

            _logger.LogInformation("Monthly statement job completed. {Count} statements generated.", count);
        }

        private class StatementRow
        {
            public Guid CustomerId { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public int ActivePolicies { get; set; }
            public decimal TotalPaidPremiums { get; set; }
            public decimal TotalPendingPremiums { get; set; }
            public int TotalClaims { get; set; }
        }
    }
}
