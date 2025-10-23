using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogCompilerBeta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompileFileController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CompileFileController> _logger;
        private readonly IFileAnalyzer _fileAnalyzer;
        private readonly IContentReader _contentReader;
        private readonly IDataRepository _dataRepository;

        public CompileFileController(ILogger<CompileFileController> logger
                                        , IFileAnalyzer fileAnalyzer
                                        , IContentReader contentReader
                                        , IDataRepository dataRepository)
        {
            _logger = logger;
            _fileAnalyzer = fileAnalyzer;
            _contentReader = contentReader;
            _dataRepository = dataRepository;
        }

        private readonly string filePath = "C:\\Assignment\\AVATAR3.messages.log";

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OriginalMessage>>> CompileFileAsync()
        {
            try
            {
                var fileContent = await _contentReader.ReadInBatchesAsync(filePath);
                var messages = await _fileAnalyzer.FindOriginalMessageAsync(fileContent);
                var originalMessages = await _dataRepository.SaveMessagesAsync(messages);

                return Ok(originalMessages);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {FilePath}", filePath);
                return NotFound(new { error = "File not found", path = filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling file: {FilePath}", filePath);
                return StatusCode(500, new { error = "An error occurred while processing the file" });
            }
        }
    }
}
