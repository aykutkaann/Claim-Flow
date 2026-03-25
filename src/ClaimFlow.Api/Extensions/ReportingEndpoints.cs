using ClaimFlow.Application.Interfaces;

namespace ClaimFlow.Api.Extensions
{
    public static class ReportingEndpoints
    {
        public static void MapReportingEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/reports");

            // Materialized view: monthly loss ratios
            group.MapGet("/loss-ratios", async (IReportingService reportingService) =>
            {
                var results = await reportingService.GetMonthlyLossRatiosAsync();
                return Results.Ok(results);
            });

            // Window function: rolling 3-month claim stats
            group.MapGet("/rolling-stats", async (IReportingService reportingService) =>
            {
                var results = await reportingService.GetRollingClaimStatsAsync();
                return Results.Ok(results);
            });

            // CTE: agent performance
            group.MapGet("/agent-performance", async (IReportingService reportingService) =>
            {
                var results = await reportingService.GetAgentPerformanceAsync();
                return Results.Ok(results);
            });

            // Full-text search on claim descriptions
            group.MapGet("/search", async (string q, IReportingService reportingService) =>
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Results.BadRequest("Search term 'q' is required.");

                var results = await reportingService.SearchClaimsAsync(q);
                return Results.Ok(results);
            });
        }
    }
}
