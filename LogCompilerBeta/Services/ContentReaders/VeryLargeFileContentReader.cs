using LogCompilerBeta.Models;
using System.Threading.Channels;

namespace LogCompilerBeta.Services.ContentReaders
{
    public class VeryLargeFileContentReader : BaseContentReader
    {
        private readonly int _batchSize;
        private readonly int _maxDegreeOfParallelism;

        public VeryLargeFileContentReader(
            ILogger<VeryLargeFileContentReader> logger,
            int batchSize = 100_000,
            int maxDegreeOfParallelism = 4) : base(logger)
        {
            _batchSize = batchSize;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public override async Task<FixMessageResult> ReadAsync(string filePath)
        {
            _logger.LogInformation("Reading very large file with channels: {FilePath}", filePath);
            return await ReadWithChannelsAsync(filePath, _batchSize, _maxDegreeOfParallelism);
        }

        public override bool CanHandle(FileInfo fileInfo)
        {
            return fileInfo.Length > GigabytesToBytes(2);
        }

        private async Task<FixMessageResult> ReadWithChannelsAsync(string filePath, int batchSize, int maxDegreeOfParallelism)
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
                            if (ContainsMessageType(fixMessage, Constants.RejectMessageType))
                            {
                                localRejects.Add(line);
                            }
                            else if (ContainsMessageType(fixMessage, Constants.OriginalMessageType))
                            {
                                localExecutions.Add(line);
                            }
                        }
                    }

                    lock (result.RejectMessages) result.RejectMessages.AddRange(localRejects);
                    lock (result.ExecutionReportMessages) result.ExecutionReportMessages.AddRange(localExecutions);
                }))
                .ToArray();

            await Task.WhenAll(consumers);
            await producer;

            return result;
        }
    }
}
