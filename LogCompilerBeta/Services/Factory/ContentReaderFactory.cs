using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Interfaces.Factory;

namespace LogCompilerBeta.Services.Factory
{
    public class ContentReaderFactory : IContentReaderFactory
    {
        private readonly IEnumerable<IContentReader> _contentReaders;
        private readonly ILogger<ContentReaderFactory> _logger;

        public ContentReaderFactory(
            IEnumerable<IContentReader> contentReaders,
            ILogger<ContentReaderFactory> logger)
        {
            _contentReaders = contentReaders;
            _logger = logger;
        }

        public IContentReader GetContentReader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileInfo = new FileInfo(filePath);

            var reader = _contentReaders.FirstOrDefault(r => r.CanHandle(fileInfo));

            if (reader == null)
            {
                _logger.LogWarning("No suitable content reader found for file: {FilePath} with size: {FileSize}",
                    filePath, fileInfo.Length);
                throw new InvalidOperationException($"No suitable content reader found for file size: {fileInfo.Length} bytes");
            }

            _logger.LogInformation("Selected {ReaderType} for file: {FilePath} with size: {FileSize} bytes",
                reader.GetType().Name, filePath, fileInfo.Length);

            return reader;
        }
    }
}
