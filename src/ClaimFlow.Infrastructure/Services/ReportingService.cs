using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ClaimFlow.Infrastructure.Services
{
    public class ReportingService : IReportingService
    {
        private readonly string _connectionString;

        public ReportingService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        /// <summary>
        /// Reads from the materialized view: monthly loss ratios.
        /// The materialized view must be created via migration and refreshed periodically.
        /// </summary>
        public async Task<List<MonthlyLossRatioDto>> GetMonthlyLossRatiosAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var results = await connection.QueryAsync<MonthlyLossRatioDto>(
                """
                SELECT
                    month AS "Month",
                    product_type AS "ProductType",
                    total_claims AS "TotalClaims",
                    total_claimed AS "TotalClaimed",
                    total_approved AS "TotalApproved",
                    total_premiums_collected AS "TotalPremiumsCollected",
                    CASE
                        WHEN total_premiums_collected > 0
                        THEN ROUND(total_approved / total_premiums_collected * 100, 2)
                        ELSE 0
                    END AS "LossRatio"
                FROM mv_monthly_loss_ratios
                ORDER BY product_type, month
                """);

            return results.ToList();
        }

        /// <summary>
        /// Window functions: rolling 3-month average claim amounts per product type.
        /// </summary>
        public async Task<List<RollingClaimStatsDto>> GetRollingClaimStatsAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var results = await connection.QueryAsync<RollingClaimStatsDto>(
                """
                WITH monthly_stats AS (
                    SELECT
                        date_trunc('month', c."SubmittedAt") AS month,
                        p."ProductType"::text AS product_type,
                        COUNT(*) AS total_claims,
                        SUM(c."ClaimedAmount") AS total_claimed,
                        SUM(c."ApprovedAmount") AS total_approved
                    FROM claims c
                    INNER JOIN policies p ON c."PolicyId" = p."Id"
                    WHERE c."SubmittedAt" >= NOW() - INTERVAL '12 months'
                    GROUP BY 1, 2
                )
                SELECT
                    month AS "Month",
                    product_type AS "ProductType",
                    total_claims AS "TotalClaims",
                    total_claimed AS "TotalClaimed",
                    COALESCE(total_approved, 0) AS "TotalApproved",
                    SUM(COALESCE(total_approved, 0)) OVER (
                        PARTITION BY product_type
                        ORDER BY month
                        ROWS BETWEEN 2 PRECEDING AND CURRENT ROW
                    ) AS "Rolling3MonthApproved",
                    CASE
                        WHEN total_claimed > 0
                        THEN ROUND(COALESCE(total_approved, 0)::numeric / total_claimed * 100, 2)
                        ELSE 0
                    END AS "ApprovalRate"
                FROM monthly_stats
                ORDER BY product_type, month
                """);

            return results.ToList();
        }

        /// <summary>
        /// CTE: agent performance report — who handled claims, approval rates, avg processing time.
        /// Uses ChangedBy from ClaimStatusHistory as the "agent".
        /// </summary>
        public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var results = await connection.QueryAsync<AgentPerformanceDto>(
                """
                WITH agent_actions AS (
                    SELECT
                        h."ChangedBy" AS agent_name,
                        h."ClaimId",
                        h."ToStatus",
                        h."ChangedAt",
                        c."SubmittedAt"
                    FROM claim_status_histories h
                    INNER JOIN claims c ON h."ClaimId" = c."Id"
                    WHERE h."ChangedBy" != 'system'
                ),
                agent_summary AS (
                    SELECT
                        agent_name,
                        COUNT(DISTINCT "ClaimId") AS total_claims_handled,
                        COUNT(*) FILTER (WHERE "ToStatus" = 'Approved') AS approved_count,
                        COUNT(*) FILTER (WHERE "ToStatus" = 'Rejected') AS rejected_count,
                        AVG(EXTRACT(EPOCH FROM ("ChangedAt" - "SubmittedAt")) / 86400) AS avg_processing_days
                    FROM agent_actions
                    GROUP BY agent_name
                )
                SELECT
                    agent_name AS "AgentName",
                    total_claims_handled AS "TotalClaimsHandled",
                    approved_count AS "ApprovedCount",
                    rejected_count AS "RejectedCount",
                    ROUND(COALESCE(avg_processing_days, 0)::numeric, 1) AS "AvgProcessingDays",
                    CASE
                        WHEN total_claims_handled > 0
                        THEN ROUND(approved_count::numeric / total_claims_handled * 100, 2)
                        ELSE 0
                    END AS "ApprovalRate"
                FROM agent_summary
                ORDER BY total_claims_handled DESC
                """);

            return results.ToList();
        }

        /// <summary>
        /// Full-text search on claim descriptions using tsvector + ts_rank.
        /// The tsvector column and GIN index must be created via migration.
        /// </summary>
        public async Task<List<ClaimSearchResultDto>> SearchClaimsAsync(string searchTerm)
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var results = await connection.QueryAsync<ClaimSearchResultDto>(
                """
                SELECT
                    c."Id" AS "ClaimId",
                    c."ClaimNumber",
                    c."Description",
                    c."Status",
                    c."ClaimedAmount",
                    c."SubmittedAt",
                    ts_rank(c."SearchVector", plainto_tsquery('english', @SearchTerm)) AS "Rank"
                FROM claims c
                WHERE c."SearchVector" @@ plainto_tsquery('english', @SearchTerm)
                ORDER BY "Rank" DESC
                LIMIT 50
                """,
                new { SearchTerm = searchTerm });

            return results.ToList();
        }
    }
}
