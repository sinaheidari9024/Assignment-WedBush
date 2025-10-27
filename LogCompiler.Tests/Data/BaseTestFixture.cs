namespace LogCompiler.Tests.Data
{
    public class BaseTestFixture : IDisposable
    {
        protected readonly List<string> _tempFiles = new List<string>();

        public string CreateTempFile(IEnumerable<string> content)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, content);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        public string CreateTempFileWithSize(long sizeInBytes)
        {
            var tempFile = Path.GetTempFileName();
            TestData.CreateLargeTestFilePrecise(tempFile, sizeInBytes);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        public string CreateTempFileWithApproximateSize(long sizeInBytes)
        {
            var tempFile = Path.GetTempFileName();
            TestData.CreateLargeTestFile(tempFile, sizeInBytes);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            GC.SuppressFinalize(this);
        }
    }

}
