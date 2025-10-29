using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services.ContentReaders
{
    public class SmallFileContentReader : BaseContentReader
    {
        public SmallFileContentReader(ILogger<SmallFileContentReader> logger) : base(logger) { }

        public override async Task<FixMessageResult> ReadAsync(string filePath)
        {
            _logger.LogInformation("Reading small file using optimized approach: {FilePath}", filePath);
            return await ReadAllAtOnceOptimizedAsync(filePath);
        }

        public override bool CanHandle(FileInfo fileInfo)
        {
            return fileInfo.Length <= MegabytesToBytes(200);
        }

        private async Task<FixMessageResult> ReadAllAtOnceOptimizedAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var result = new FixMessageResult();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                var fixMessage = ExtractFixMessage(line);
                if (string.IsNullOrEmpty(fixMessage)) continue;

                if (ContainsMessageType(fixMessage, Constants.RejectMessageType))
                {
                    result.RejectMessages.Add(line);
                }
                else if (ContainsMessageType(fixMessage, Constants.OriginalMessageType))
                {
                    result.ExecutionReportMessages.Add(line);
                }
            }

            _logger.LogInformation("Read {RejectCount} reject messages and {ExecutionReportCount} execution report messages",
                result.RejectMessages.Count, result.ExecutionReportMessages.Count);

            return result;
        }
    }
}