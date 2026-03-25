using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClaimFlow.Infrastructure.Services
{
    public class FraudDetectionService : IFraudDetectionService
    {
        private readonly string _connectionString;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<FraudDetectionService> _logger;

        public FraudDetectionService(
            IConfiguration configuration,
            IEmbeddingService embeddingService,
            ILogger<FraudDetectionService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task<FraudCheckResult> CheckFraudAsync(Guid claimId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Get the claim we're checking (exclude Embedding — Dapper can't map pgvector)
            var claim = await connection.QueryFirstOrDefaultAsync<ClaimRow>(
                """SELECT "Id", "Description", "ClaimedAmount", "PolicyId" FROM claims WHERE "Id" = @ClaimId""",
                new { ClaimId = claimId });

            // Get embedding as string separately for similarity queries
            var embeddingString = await connection.QueryFirstOrDefaultAsync<string>(
                """SELECT "Embedding"::text FROM claims WHERE "Id" = @ClaimId AND "Embedding" IS NOT NULL""",
                new { ClaimId = claimId });

            if (claim == null)
                throw new Exception($"Claim {claimId} not found.");

            var riskFactors = new List<string>();
            int totalScore = 0;

            // --- Factor 1: Vector similarity to known fraud (0-40 points) ---
            var similarityScore = await CalculateVectorSimilarityScore(connection, claim, embeddingString);
            totalScore += similarityScore.Score;
            riskFactors.AddRange(similarityScore.Factors);

            // --- Factor 2: Claim amount vs policy history (0-20 points) ---
            var amountScore = await CalculateAmountAnomalyScore(connection, claim);
            totalScore += amountScore.Score;
            riskFactors.AddRange(amountScore.Factors);

            // --- Factor 3: Time since policy purchase (0-20 points) ---
            var timingScore = await CalculateTimingScore(connection, claim);
            totalScore += timingScore.Score;
            riskFactors.AddRange(timingScore.Factors);

            // --- Factor 4: Claim frequency (0-20 points) ---
            var frequencyScore = await CalculateFrequencyScore(connection, claim);
            totalScore += frequencyScore.Score;
            riskFactors.AddRange(frequencyScore.Factors);

            _logger.LogInformation(
                "Fraud check for claim {ClaimId}: score={Score}, factors={FactorCount}",
                claimId, totalScore, riskFactors.Count);

            return new FraudCheckResult(
                totalScore,
                similarityScore.SimilarClaims,
                riskFactors);
        }

        private async Task<(int Score, List<string> Factors, List<SimilarFraudClaim> SimilarClaims)>
            CalculateVectorSimilarityScore(NpgsqlConnection connection, ClaimRow claim, string? embeddingString)
        {
            var factors = new List<string>();
            var similarClaims = new List<SimilarFraudClaim>();
            int score = 0;

            if (embeddingString == null)
                return (score, factors, similarClaims);

            var results = await connection.QueryAsync<SimilarClaimRow>(
                """
                SELECT c."Id" AS ClaimId, c."Description",
                    1 - (c."Embedding" <=> @Embedding::vector) AS SimilarityScore
                FROM claims c
                WHERE c."IsFraud" = true AND c."Id" != @ClaimId
                ORDER BY c."Embedding" <=> @Embedding::vector
                LIMIT 5
                """,
                new { Embedding = embeddingString, ClaimId = claim.Id });

            foreach (var result in results)
            {
                similarClaims.Add(new SimilarFraudClaim(
                    result.ClaimId, result.Description, result.SimilarityScore));

                if (result.SimilarityScore > 0.85)
                {
                    score = 40;
                    factors.Add($"Very high similarity ({result.SimilarityScore:P0}) to known fraud claim.");
                }
                else if (result.SimilarityScore > 0.70)
                {
                    score = Math.Max(score, 25);
                    factors.Add($"Moderate similarity ({result.SimilarityScore:P0}) to known fraud claim.");
                }
                else if (result.SimilarityScore > 0.55)
                {
                    score = Math.Max(score, 10);
                    factors.Add($"Low similarity ({result.SimilarityScore:P0}) to known fraud claim.");
                }
            }

            return (score, factors, similarClaims);
        }

        private async Task<(int Score, List<string> Factors)>
            CalculateAmountAnomalyScore(NpgsqlConnection connection, ClaimRow claim)
        {
            var factors = new List<string>();
            int score = 0;

            var avgAmount = await connection.QueryFirstOrDefaultAsync<decimal?>(
                """
                SELECT AVG("ClaimedAmount")
                FROM claims
                WHERE "PolicyId" = @PolicyId AND "Id" != @ClaimId
                """,
                new { claim.PolicyId, ClaimId = claim.Id });

            if (avgAmount.HasValue && avgAmount.Value > 0)
            {
                var ratio = claim.ClaimedAmount / avgAmount.Value;
                if (ratio >= 3)
                {
                    score = 20;
                    factors.Add($"Claimed amount is {ratio:F1}x the average for this policy.");
                }
                else if (ratio >= 2)
                {
                    score = 10;
                    factors.Add($"Claimed amount is {ratio:F1}x the average for this policy.");
                }
            }

            return (score, factors);
        }

        private async Task<(int Score, List<string> Factors)>
            CalculateTimingScore(NpgsqlConnection connection, ClaimRow claim)
        {
            var factors = new List<string>();
            int score = 0;

            var policyStartDate = await connection.QueryFirstOrDefaultAsync<DateTime?>(
                """SELECT "StartDate" FROM policies WHERE "Id" = @PolicyId""",
                new { claim.PolicyId });

            if (policyStartDate.HasValue)
            {
                var daysSincePurchase = (DateTime.UtcNow - policyStartDate.Value).TotalDays;
                if (daysSincePurchase <= 30)
                {
                    score = 20;
                    factors.Add($"Claim filed only {daysSincePurchase:F0} days after policy purchase.");
                }
                else if (daysSincePurchase <= 90)
                {
                    score = 10;
                    factors.Add($"Claim filed {daysSincePurchase:F0} days after policy purchase (within 90-day window).");
                }
            }

            return (score, factors);
        }

        private async Task<(int Score, List<string> Factors)>
            CalculateFrequencyScore(NpgsqlConnection connection, ClaimRow claim)
        {
            var factors = new List<string>();
            int score = 0;

            var claimCount = await connection.QueryFirstOrDefaultAsync<int>(
                """
                SELECT COUNT(*)
                FROM claims
                WHERE "PolicyId" = @PolicyId
                AND "SubmittedAt" >= NOW() - INTERVAL '6 months'
                """,
                new { claim.PolicyId });

            if (claimCount > 3)
            {
                score = 20;
                factors.Add($"{claimCount} claims filed on this policy in the last 6 months.");
            }
            else if (claimCount > 2)
            {
                score = 10;
                factors.Add($"{claimCount} claims filed on this policy in the last 6 months.");
            }

            return (score, factors);
        }

        // Internal row types for Dapper mapping
        private class ClaimRow
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = "";
            public decimal ClaimedAmount { get; set; }
            public Guid PolicyId { get; set; }
        }

        private class SimilarClaimRow
        {
            public Guid ClaimId { get; set; }
            public string Description { get; set; } = "";
            public double SimilarityScore { get; set; }
        }
    }
}
