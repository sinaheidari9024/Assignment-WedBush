using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IFileAnalyzer
    {
        Task<List<string>> FindOriginalMessageAsync(FixMessageResult fixResult);
    }
}
