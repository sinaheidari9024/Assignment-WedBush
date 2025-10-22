using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services
{
    public class ContentReader : IContentReader
    {
        private readonly ILogger<ContentReader> _logger;

        public ContentReader(ILogger<ContentReader> logger)
        {
            _logger = logger;
        }

        public async Task<FixMessageResult> ReadAllAtOnceOptimizedAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var result = new FixMessageResult();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                var fixMessage = ExtractFixMessage(line);
                if (string.IsNullOrEmpty(fixMessage)) continue;

                if (ContainsMessageType(fixMessage, "3"))
                {
                    result.RejectMessages.Add(line);
                }
                else if (ContainsMessageType(fixMessage, "8"))
                {
                    result.ExecutionReportMessages.Add(line);
                }
            }

            _logger.LogInformation("Read {RejectCount} reject messages and {ExecutionReportCount} execution report messages",
                result.RejectMessages.Count, result.ExecutionReportMessages.Count);

            return result;

        }

        public async Task<FixMessageResult> ReadInBatchesAsync(string filePath, int batchSize = 10_000)
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

                    // Extract and check FIX message type
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
                        await Task.Yield(); // Prevent blocking
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

        private static string ExtractFixMessage(string line)
        {
            int fixStart = line.IndexOf("8=FIX");
            return fixStart >= 0 ? line.Substring(fixStart) : string.Empty;
        }

        private static bool ContainsMessageType(string fixMessage, string messageType)
        {
            var pattern = $"|35={messageType}";
            return fixMessage.Contains(pattern + "|") || fixMessage.EndsWith(pattern);
        }
    }
}
