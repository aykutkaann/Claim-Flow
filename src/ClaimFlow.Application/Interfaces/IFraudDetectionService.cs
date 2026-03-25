using ClaimFlow.Application.DTOs;

namespace ClaimFlow.Application.Interfaces
{
    public interface IFraudDetectionService
    {
        Task<FraudCheckResult> CheckFraudAsync(Guid claimId);
    }
}
