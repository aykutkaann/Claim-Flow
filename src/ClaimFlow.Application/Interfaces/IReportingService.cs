using ClaimFlow.Application.DTOs;

namespace ClaimFlow.Application.Interfaces
{
    public interface IReportingService
    {
        Task<List<MonthlyLossRatioDto>> GetMonthlyLossRatiosAsync();
        Task<List<RollingClaimStatsDto>> GetRollingClaimStatsAsync();
        Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
        Task<List<ClaimSearchResultDto>> SearchClaimsAsync(string searchTerm);
    }
}
