using LogCompiler.Tests.Data;
using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Services.ContentReaders;
using LogCompilerBeta.Services.Factory;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    public class ContentReaderFactoryTests : BaseTestFixture
    {
        private readonly Mock<ILogger<ContentReaderFactory>> _mockLogger;

        public ContentReaderFactoryTests()
        {
            _mockLogger = new Mock<ILogger<ContentReaderFactory>>();
        }

        //[Theory]
        //[InlineData(100 * 1024 * 1024, typeof(SmallFileContentReader))]      // 100MB
        //[InlineData(500 * 1024 * 1024, typeof(MediumFileContentReader))]     // 500MB
        //[InlineData(1500 * 1024 * 1024, typeof(LargeFileContentReader))]     // 1.5GB
        //[InlineData(3L * 1024 * 1024 * 1024, typeof(VeryLargeFileContentReader))] // 3GB
        //public void GetContentReader_WithVariousFileSizes_ReturnsCorrectReader(long fileSize, Type expectedType)
        //{
        //    // Arrange
        //    var readers = new List<IContentReader>
        //{
        //    new SmallFileContentReader(Mock.Of<ILogger<SmallFileContentReader>>()),
        //    new MediumFileContentReader(Mock.Of<ILogger<MediumFileContentReader>>()),
        //    new LargeFileContentReader(Mock.Of<ILogger<LargeFileContentReader>>()),
        //    new VeryLargeFileContentReader(Mock.Of<ILogger<VeryLargeFileContentReader>>())
        //};

        //    var factory = new ContentReaderFactory(readers, _mockLogger.Object);
        //    var filePath = CreateTempFileWithSize(fileSize);

        //    // Act
        //    var result = factory.GetContentReader(filePath);

        //    // Assert
        //    Assert.IsType(expectedType, result);
        //}

        [Fact]
        public void GetContentReader_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var readers = new List<IContentReader>();
            var factory = new ContentReaderFactory(readers, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => factory.GetContentReader(@"C:\nonexistent\file.txt"));
        }

        [Fact]
        public void GetContentReader_WithNoSuitableReader_ThrowsInvalidOperationException()
        {
            // Arrange
            var readers = new List<IContentReader>(); // Empty list
            var factory = new ContentReaderFactory(readers, _mockLogger.Object);
            var filePath = CreateTempFile(new[] { "test" });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => factory.GetContentReader(filePath));
        }

        [Fact]
        public void GetContentReader_WithNullFilePath_ThrowsFileNotFoundException()
        {
            // Arrange
            var readers = new List<IContentReader>();
            var factory = new ContentReaderFactory(readers, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => factory.GetContentReader(null!));
        }
    }
}
