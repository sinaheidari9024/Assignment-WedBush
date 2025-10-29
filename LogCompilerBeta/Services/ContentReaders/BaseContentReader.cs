using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services.ContentReaders
{
    public abstract class BaseContentReader : IContentReader
    {
        protected readonly ILogger<BaseContentReader> _logger;

        protected BaseContentReader(ILogger<BaseContentReader> logger)
        {
            _logger = logger;
        }

        public abstract Task<FixMessageResult> ReadAsync(string filePath);
        public abstract bool CanHandle(FileInfo fileInfo);

        protected virtual long GetFileSizeInBytes(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        protected virtual long MegabytesToBytes(long megabytes) => megabytes * 1024 * 1024;
        protected virtual long GigabytesToBytes(long gigabytes) => gigabytes * 1024 * 1024 * 1024;

        protected Task ProcessBatchParallel(List<string> batch, FixMessageResult result)
        {
            return Task.Run(() =>
            {
                var batchRejects = new List<string>();
                var batchExecutions = new List<string>();

                Parallel.ForEach(batch, line =>
                {
                    var fixMessage = ExtractFixMessage(line);
                    if (!string.IsNullOrEmpty(fixMessage))
                    {
                        if (ContainsMessageType(fixMessage, Constants.RejectMessageType))
                        {
                            lock (batchRejects) batchRejects.Add(line);
                        }
                        else if (ContainsMessageType(fixMessage, Constants.OriginalMessageType))
                        {
                            lock (batchExecutions) batchExecutions.Add(line);
                        }
                    }
                });

                lock (result.RejectMessages) result.RejectMessages.AddRange(batchRejects);
                lock (result.ExecutionReportMessages) result.ExecutionReportMessages.AddRange(batchExecutions);
            });
        }

        protected string ExtractFixMessage(string line)
        {
            int fixStart = line.IndexOf("8=FIX");
            return fixStart >= 0 ? line.Substring(fixStart) : string.Empty;
        }

        protected bool ContainsMessageType(string fixMessage, string messageType)
        {
            var pattern = $"|35={messageType}";
            return fixMessage.Contains(pattern + "|") || fixMessage.EndsWith(pattern);
        }
    }
}