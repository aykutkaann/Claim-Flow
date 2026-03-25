namespace ClaimFlow.Application.DTOs
{
    // Materialized view: monthly loss ratio (claims paid vs premiums collected)
    public record MonthlyLossRatioDto(
        DateTime Month,
        string ProductType,
        int TotalClaims,
        decimal TotalClaimed,
        decimal TotalApproved,
        decimal TotalPremiumsCollected,
        decimal LossRatio);

    // Window function: rolling 3-month stats per product type
    public record RollingClaimStatsDto(
        DateTime Month,
        string ProductType,
        int TotalClaims,
        decimal TotalClaimed,
        decimal TotalApproved,
        decimal Rolling3MonthApproved,
        decimal ApprovalRate);

    // CTE: hierarchical performance report
    public record AgentPerformanceDto(
        string AgentName,
        int TotalClaimsHandled,
        int ApprovedCount,
        int RejectedCount,
        decimal AvgProcessingDays,
        decimal ApprovalRate);

    // Full-text search result
    public record ClaimSearchResultDto(
        Guid ClaimId,
        string ClaimNumber,
        string Description,
        string Status,
        decimal ClaimedAmount,
        DateTime SubmittedAt,
        double Rank);
}
