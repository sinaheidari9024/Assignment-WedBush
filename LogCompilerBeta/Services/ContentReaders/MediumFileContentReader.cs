using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services.ContentReaders
{
    public class MediumFileContentReader : BaseContentReader
    {
        private readonly int _batchSize;

        public MediumFileContentReader(ILogger<MediumFileContentReader> logger, int batchSize = 10_000)
            : base(logger)
        {
            _batchSize = batchSize;
        }

        public override async Task<FixMessageResult> ReadAsync(string filePath)
        {
            _logger.LogInformation("Reading medium file in batches: {FilePath}", filePath);
            return await ReadInBatchesAsync(filePath, _batchSize);
        }

        public override bool CanHandle(FileInfo fileInfo)
        {
            return fileInfo.Length > MegabytesToBytes(200) &&
                   fileInfo.Length <= GigabytesToBytes(1);
        }

        private async Task<FixMessageResult> ReadInBatchesAsync(string filePath, int batchSize)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            var result = new FixMessageResult();
            var batchCounter = 0;

            try
            {
                using var reader = new StreamReader(filePath);

                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    var fixMessage = ExtractFixMessage(line);
                    if (!string.IsNullOrEmpty(fixMessage))
                    {
                        if (ContainsMessageType(fixMessage, "3"))
                        {
                            result.RejectMessages.Add(line);
                        }
                        else if (ContainsMessageType(fixMessage, "8"))
                        {
                            result.ExecutionReportMessages.Add(line);
                        }
                    }

                    batchCounter++;
                    if (batchCounter >= batchSize)
                    {
                        batchCounter = 0;
                        await Task.Yield();
                    }
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