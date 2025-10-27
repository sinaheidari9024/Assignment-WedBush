using LogCompilerBeta.Models;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    public class BaseContentReaderTests
    {
        [Theory]
        [InlineData("8=FIX.4.2|35=3|49=TEST", "3", true)]
        [InlineData("8=FIX.4.2|35=8|49=TEST", "8", true)]
        [InlineData("8=FIX.4.2|35=A|49=TEST", "3", false)]
        [InlineData("Some text 8=FIX.4.2|35=3|49=TEST", "3", true)]
        [InlineData("Invalid message", "3", false)]
        [InlineData("", "3", false)]
        public void ContainsMessageType_WithVariousInputs_ReturnsExpectedResult(string fixMessage, string messageType, bool expected)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseContentReader>>();
            var concreteReader = new TestableContentReader(mockLogger.Object);

            // Act
            var result = concreteReader.PublicContainsMessageType(fixMessage, messageType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Prefix 8=FIX.4.2|35=3", "8=FIX.4.2|35=3")]
        [InlineData("8=FIX.4.2|35=3", "8=FIX.4.2|35=3")]
        [InlineData("No FIX message", "")]
        [InlineData("", "")]
        public void ExtractFixMessage_WithVariousInputs_ReturnsExpectedResult(string line, string expected)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseContentReader>>();
            var concreteReader = new TestableContentReader(mockLogger.Object);

            // Act
            var result = concreteReader.PublicExtractFixMessage(line);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MegabytesToBytes_ConvertsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseContentReader>>();
            var concreteReader = new TestableContentReader(mockLogger.Object);

            // Act
            var result = concreteReader.PublicMegabytesToBytes(1);

            // Assert
            Assert.Equal(1048576L, result);
        }

        [Fact]
        public void GigabytesToBytes_ConvertsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseContentReader>>();
            var concreteReader = new TestableContentReader(mockLogger.Object);

            // Act
            var result = concreteReader.PublicGigabytesToBytes(1);

            // Assert
            Assert.Equal(1073741824L, result);
        }

        // Testable concrete implementation for testing protected methods
        private class TestableContentReader : BaseContentReader
        {
            public TestableContentReader(ILogger<BaseContentReader> logger) : base(logger) { }

            public override Task<FixMessageResult> ReadAsync(string filePath) => Task.FromResult(new FixMessageResult());
            public override bool CanHandle(FileInfo fileInfo) => true;

            public bool PublicContainsMessageType(string fixMessage, string messageType)
                => ContainsMessageType(fixMessage, messageType);

            public string PublicExtractFixMessage(string line) => ExtractFixMessage(line);
            public long PublicMegabytesToBytes(long megabytes) => MegabytesToBytes(megabytes);
            public long PublicGigabytesToBytes(long gigabytes) => GigabytesToBytes(gigabytes);
        }
    }
}
