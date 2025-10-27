using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services.ContentReaders
{
    public class LargeFileContentReader : BaseContentReader
    {
        private readonly int _batchSize;

        public LargeFileContentReader(ILogger<LargeFileContentReader> logger, int batchSize = 100_000)
            : base(logger)
        {
            _batchSize = batchSize;
        }

        public override async Task<FixMessageResult> ReadAsync(string filePath)
        {
            _logger.LogInformation("Reading large file with parallel batches: {FilePath}", filePath);
            return await ReadInBatchesParallelAsync(filePath, _batchSize);
        }

        public override bool CanHandle(FileInfo fileInfo)
        {
            return fileInfo.Length > GigabytesToBytes(1) &&
                   fileInfo.Length <= GigabytesToBytes(2);
        }

        private async Task<FixMessageResult> ReadInBatchesParallelAsync(string filePath, int batchSize)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            var linesBatch = new List<string>(batchSize);
            var result = new FixMessageResult();

            try
            {
                using var reader = new StreamReader(filePath);

                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    linesBatch.Add(line);

                    if (linesBatch.Count >= batchSize)
                    {
                        await ProcessBatchParallel(linesBatch, result);
                        linesBatch.Clear();
                        await Task.Yield();
                    }
                }

                if (linesBatch.Count > 0)
                {
                    await ProcessBatchParallel(linesBatch, result);
                }

                _logger.LogInformation("Processed file: {FilePath}. Found {RejectCount} rejects and {ExecReportCount} execution reports",
                    filePath, result.RejectMessages.Count, result.ExecutionReportMessages.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
                throw;
            }
        }
    }

}
