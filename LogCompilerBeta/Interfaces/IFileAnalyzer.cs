using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IFileAnalyzer
    {
        List<string> FindOriginalMessage(FixMessageResult fixResult);
    }
}
