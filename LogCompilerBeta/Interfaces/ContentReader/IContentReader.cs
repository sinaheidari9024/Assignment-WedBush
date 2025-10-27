using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces.ContentReader
{
    public interface IContentReader
    {
        Task<FixMessageResult> ReadAsync(string filePath);
        bool CanHandle(FileInfo fileInfo);

    }
}
