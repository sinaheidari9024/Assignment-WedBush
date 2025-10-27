using LogCompilerBeta.Interfaces.ContentReader;

namespace LogCompilerBeta.Interfaces.Factory
{
    public interface IContentReaderFactory
    {
        IContentReader GetContentReader(string filePath);
    }
}
