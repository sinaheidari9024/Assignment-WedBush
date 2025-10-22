using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;

namespace LogCompilerBeta.Services
{
    public class FileAnalyzer : IFileAnalyzer
    {
        private readonly ILogger<FileAnalyzer> _logger;

        public FileAnalyzer(ILogger<FileAnalyzer> logger)
        {
            _logger = logger;
        }

        public async Task<List<string>> FindOriginalMessageAsync(FixMessageResult fixResult)
        {
            return await Task.Run(() =>
            {
                var matchedOriginalReports = new List<string>();

                foreach (var rejectMessage in fixResult.RejectMessages)
                {
                    int? tag45Value = ExtractTagValue(rejectMessage, 45);
                    if (tag45Value == null) continue;

                    var matchingExecutionReport = fixResult.ExecutionReportMessages
                        .FirstOrDefault(er => ExtractTagValue(er, 34) == tag45Value);

                    if (matchingExecutionReport != null)
                    {
                        matchedOriginalReports.Add(matchingExecutionReport);
                        fixResult.ExecutionReportMessages.Remove(matchingExecutionReport);
                    }
                }

                _logger.LogInformation("Found {MatchedCount} matching execution reports for rejects",
                    matchedOriginalReports.Count);

                return matchedOriginalReports;
            });
        }

        private int? ExtractTagValue(string fixMessage, int tagNumber)
        {
            try
            {
                var tagPattern = $"|{tagNumber}=";
                int tagIndex = fixMessage.IndexOf(tagPattern);

                if (tagIndex >= 0)
                {
                    int valueStart = tagIndex + tagPattern.Length;
                    int valueEnd = fixMessage.IndexOf('|', valueStart);

                    if (valueEnd == -1)
                    {
                        valueEnd = fixMessage.Length;
                    }

                    string valueStr = fixMessage.Substring(valueStart, valueEnd - valueStart);

                    if (int.TryParse(valueStr, out int value))
                    {
                        return value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract tag {TagNumber} from message: {Message}",
                    tagNumber, fixMessage);
                return null;
            }
        }
    }
}
