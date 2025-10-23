using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using System.Threading.Channels;

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

        public async Task<FixMessageResult> ReadInBatchesAsync(string filePath, int batchSize = 100_000)
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

        public async Task<FixMessageResult> ReadInBatchesParallelAsync(string filePath, int batchSize = 100_000)
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

        public async Task<FixMessageResult> ReadWithChannelsAsync(string filePath, int batchSize = 100_000, int maxDegreeOfParallelism = 4)
        {
            var channel = Channel.CreateBounded<string>(batchSize * 2);
            var result = new FixMessageResult();

            var producer = Task.Run(async () =>
            {
                using var reader = new StreamReader(filePath);
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        await channel.Writer.WriteAsync(line);
                    }
                }
                channel.Writer.Complete();
            });

            
            var consumers = Enumerable.Range(0, maxDegreeOfParallelism)
                .Select(_ => Task.Run(async () =>
                {
                    var localRejects = new List<string>();
                    var localExecutions = new List<string>();

                    await foreach (var line in channel.Reader.ReadAllAsync())
                    {
                        var fixMessage = ExtractFixMessage(line);
                        if (!string.IsNullOrEmpty(fixMessage))
                        {
                            if (ContainsMessageType(fixMessage, "3"))
                            {
                                localRejects.Add(line);
                            }
                            else if (ContainsMessageType(fixMessage, "8"))
                            {
                                localExecutions.Add(line);
                            }
                        }
                    }

                    // Merge results
                    lock (result.RejectMessages) result.RejectMessages.AddRange(localRejects);
                    lock (result.ExecutionReportMessages) result.ExecutionReportMessages.AddRange(localExecutions);
                }))
                .ToArray();

            await Task.WhenAll(consumers);
            await producer;

            return result;
        }

        private Task ProcessBatchParallel(List<string> batch, FixMessageResult result)
        {
            return Task.Run(() =>
            {
                var batchRejects = new List<string>();
                var batchExecutions = new List<string>();

                // Parallel processing for CPU-bound work
                Parallel.ForEach(batch, line =>
                {
                    var fixMessage = ExtractFixMessage(line);
                    if (!string.IsNullOrEmpty(fixMessage))
                    {
                        if (ContainsMessageType(fixMessage, "3"))
                        {
                            lock (batchRejects) batchRejects.Add(line);
                        }
                        else if (ContainsMessageType(fixMessage, "8"))
                        {
                            lock (batchExecutions) batchExecutions.Add(line);
                        }
                    }
                });

                // Thread-safe addition to final result
                lock (result.RejectMessages) result.RejectMessages.AddRange(batchRejects);
                lock (result.ExecutionReportMessages) result.ExecutionReportMessages.AddRange(batchExecutions);
            });
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
