using ClaimFlow.Application.Interfaces;

namespace ClaimFlow.Infrastructure.Services
{
    public class FakeEmbeddingService : IEmbeddingService
    {
        public Task<float[]> GetEmbeddingAsync(string text)
        {
            // Fake implementation — generates deterministic-ish embeddings based on text hash
            // so the same description always produces the same vector (useful for testing similarity)
            var seed = text.GetHashCode();
            var random = new Random(seed);
            var embedding = Enumerable.Range(0, 1536)
                .Select(_ => (float)(random.NextDouble() * 2 - 1))
                .ToArray();

            return Task.FromResult(embedding);
        }
    }
}
