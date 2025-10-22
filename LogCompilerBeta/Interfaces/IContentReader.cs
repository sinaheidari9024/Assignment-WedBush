using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IContentReader
    {
        Task<FixMessageResult> ReadAllAtOnceOptimizedAsync(string filePath);
        Task<FixMessageResult> ReadInBatchesAsync(string filePath, int batchSize = 10_000);
    }
}
