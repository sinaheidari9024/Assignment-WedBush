using FluentAssertions;
using LogCompilerBeta.Controllers;
using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Controllers
{
    public class CompileFileControllerTests
    {
        private readonly CompileFileController _sut;
        private readonly Mock<ILogger<CompileFileController>> _loggerMock;
        private readonly Mock<IFileAnalyzer> _fileAnalyzerMock;
        private readonly Mock<IContentReader> _contentReaderMock;
        private readonly Mock<IDataRepository> _dataRepositoryMock;

        public CompileFileControllerTests()
        {
            _loggerMock = new Mock<ILogger<CompileFileController>>();
            _fileAnalyzerMock = new Mock<IFileAnalyzer>();
            _contentReaderMock = new Mock<IContentReader>();
            _dataRepositoryMock = new Mock<IDataRepository>();

            _sut = new CompileFileController(
                _loggerMock.Object,
                _fileAnalyzerMock.Object,
                _contentReaderMock.Object,
                _dataRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CompileFileAsync_SuccessfulProcessing_ReturnsOkWithMessages()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>
                {
                    "2024-01-01 10:00:00 8=FIX.4.4|9=120|35=3|45=100|49=Broker|56=Market|",
                    "2024-01-01 10:00:01 8=FIX.4.4|9=130|35=3|45=101|49=Broker|56=Market|"
                },
                ExecutionReportMessages = new List<string>
                {
                    "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|",
                    "2024-01-01 10:00:00 8=FIX.4.4|9=160|35=8|34=101|49=Broker|56=Market|",
                    "2024-01-01 10:00:02 8=FIX.4.4|9=170|35=8|34=102|49=Broker|56=Market|"
                }
            };

            var analyzedMessages = new List<string>
            {
                "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|",
                "2024-01-01 10:00:00 8=FIX.4.4|9=160|35=8|34=101|49=Broker|56=Market|"
            };

            var savedMessages = new List<OriginalMessage>
            {
                new OriginalMessage { Id = 1, Message = analyzedMessages[0], CreatedAt = DateTime.UtcNow },
                new OriginalMessage { Id = 2, Message = analyzedMessages[1], CreatedAt = DateTime.UtcNow }
            };

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);

            // Verify method calls
            _contentReaderMock.Verify(x => x.ReadAllAtOnceOptimizedAsync("C:\\Assignment\\AVATAR3.messages.log"), Times.Once);
            _fileAnalyzerMock.Verify(x => x.FindOriginalMessageAsync(fixMessageResult), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveMessagesAsync(analyzedMessages), Times.Once);

        }

        [Fact]
        public async Task CompileFileAsync_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Value.Should().BeEquivalentTo(new { error = "File not found", path = "C:\\Assignment\\AVATAR3.messages.log" });

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("File not found") && v.ToString().Contains("C:\\Assignment\\AVATAR3.messages.log")),
                    It.IsAny<FileNotFoundException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task CompileFileAsync_GeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Database connection failed");
            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { error = "An error occurred while processing the file" });

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error compiling file") && v.ToString().Contains("C:\\Assignment\\AVATAR3.messages.log")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task CompileFileAsync_NoMatchingMessages_ReturnsEmptyList()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>
                {
                    "2024-01-01 10:00:00 8=FIX.4.4|9=120|35=3|45=999|49=Broker|56=Market|" // No matching execution report
                },
                ExecutionReportMessages = new List<string>
                {
                    "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|", // No matching reject
                    "2024-01-01 10:00:00 8=FIX.4.4|9=160|35=8|34=101|49=Broker|56=Market|"  // No matching reject
                }
            };

            var analyzedMessages = new List<string>(); // Empty list - no matches found
            var savedMessages = new List<OriginalMessage>(); // Empty list

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);
            (okResult.Value as List<OriginalMessage>).Should().BeEmpty();
        }

        [Fact]
        public async Task CompileFileAsync_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>(),
                ExecutionReportMessages = new List<string>()
            };

            var analyzedMessages = new List<string>();
            var savedMessages = new List<OriginalMessage>();

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);
        }

        [Fact]
        public async Task CompileFileAsync_OnlyRejectMessages_ReturnsEmptyList()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>
                {
                    "2024-01-01 10:00:00 8=FIX.4.4|9=120|35=3|45=100|49=Broker|56=Market|",
                    "2024-01-01 10:00:01 8=FIX.4.4|9=130|35=3|45=101|49=Broker|56=Market|"
                },
                ExecutionReportMessages = new List<string>() // No execution reports
            };

            var analyzedMessages = new List<string>(); // No matches found
            var savedMessages = new List<OriginalMessage>();

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);
        }

        [Fact]
        public async Task CompileFileAsync_OnlyExecutionReports_ReturnsEmptyList()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>(), // No reject messages
                ExecutionReportMessages = new List<string>
                {
                    "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|",
                    "2024-01-01 10:00:00 8=FIX.4.4|9=160|35=8|34=101|49=Broker|56=Market|"
                }
            };

            var analyzedMessages = new List<string>(); // No matches found
            var savedMessages = new List<OriginalMessage>();

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);
        }

        [Fact]
        public async Task CompileFileAsync_DataRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>
                {
                    "2024-01-01 10:00:00 8=FIX.4.4|9=120|35=3|45=100|49=Broker|56=Market|"
                },
                ExecutionReportMessages = new List<string>
                {
                    "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|"
                }
            };

            var analyzedMessages = new List<string> { "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|" };

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ThrowsAsync(new Exception("Database save failed"));

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { error = "An error occurred while processing the file" });
        }

        [Fact]
        public async Task CompileFileAsync_FileAnalyzerThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string> { "2024-01-01 10:00:00 8=FIX.4.4|9=120|35=3|45=100|49=Broker|56=Market|" },
                ExecutionReportMessages = new List<string> { "2024-01-01 09:59:59 8=FIX.4.4|9=150|35=8|34=100|49=Broker|56=Market|" }
            };

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ThrowsAsync(new Exception("Analysis failed"));

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { error = "An error occurred while processing the file" });
        }

        [Fact]
        public async Task CompileFileAsync_MultipleMatchingMessages_ReturnsAllMatches()
        {
            // Arrange
            var fixMessageResult = new FixMessageResult
            {
                RejectMessages = new List<string>
                {
                    "2024-01-01 10:00:00 8=FIX.4.4|35=3|45=100|",
                    "2024-01-01 10:00:01 8=FIX.4.4|35=3|45=101|",
                    "2024-01-01 10:00:02 8=FIX.4.4|35=3|45=102|"
                },
                ExecutionReportMessages = new List<string>
                {
                    "2024-01-01 09:59:59 8=FIX.4.4|35=8|34=100|",
                    "2024-01-01 10:00:00 8=FIX.4.4|35=8|34=101|",
                    "2024-01-01 10:00:01 8=FIX.4.4|35=8|34=102|",
                    "2024-01-01 10:00:03 8=FIX.4.4|35=8|34=103|" // No matching reject
                }
            };

            var analyzedMessages = new List<string>
            {
                "2024-01-01 09:59:59 8=FIX.4.4|35=8|34=100|",
                "2024-01-01 10:00:00 8=FIX.4.4|35=8|34=101|",
                "2024-01-01 10:00:01 8=FIX.4.4|35=8|34=102|"
            };

            var savedMessages = analyzedMessages.Select((msg, index) => new OriginalMessage
            {
                Id = index + 1,
                Message = msg,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _contentReaderMock
                .Setup(x => x.ReadAllAtOnceOptimizedAsync(It.IsAny<string>()))
                .ReturnsAsync(fixMessageResult);

            _fileAnalyzerMock
                .Setup(x => x.FindOriginalMessageAsync(fixMessageResult))
                .ReturnsAsync(analyzedMessages);

            _dataRepositoryMock
                .Setup(x => x.SaveMessagesAsync(analyzedMessages))
                .ReturnsAsync(savedMessages);

            // Act
            var result = await _sut.CompileFileAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(savedMessages);
            (okResult.Value as List<OriginalMessage>).Should().HaveCount(3);
        }
    }
}