namespace ClaimFlow.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }
}
