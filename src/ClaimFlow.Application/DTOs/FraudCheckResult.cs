namespace ClaimFlow.Application.DTOs
{
    public record FraudCheckResult(
        int FraudRiskScore,
        List<SimilarFraudClaim> SimilarFraudulentClaims,
        List<string> RiskFactors);

    public record SimilarFraudClaim(
        Guid ClaimId,
        string Description,
        double SimilarityScore);
}
