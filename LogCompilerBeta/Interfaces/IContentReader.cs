using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IContentReader
    {
        Task<FixMessageResult> ReadAllAtOnceOptimizedAsync(string filePath);
        Task<FixMessageResult> ReadInBatchesAsync(string filePath, int batchSize = 10_000);
        Task<FixMessageResult> ReadInBatchesParallelAsync(string filePath, int batchSize = 100_000);
        Task<FixMessageResult> ReadWithChannelsAsync(string filePath, int batchSize = 100_000, int maxDegreeOfParallelism = 4);

        Task<FixMessageResult> ReadAllAtOnceOptimizedAsync(IFormFile file);
        Task<FixMessageResult> ReadInBatchesAsync(IFormFile file, int batchSize = 100_000);
        Task<FixMessageResult> ReadInBatchesParallelAsync(IFormFile file, int batchSize = 100_000);
        Task<FixMessageResult> ReadWithChannelsAsync(IFormFile file, int batchSize = 100_000, int maxDegreeOfParallelism = 4);
    }
}
