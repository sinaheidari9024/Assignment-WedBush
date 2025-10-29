using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Interfaces.Factory;
using LogCompilerBeta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
public class CompileFileController : ControllerBase
{
    private readonly IFileAnalyzer _fileAnalyzer;
    private readonly IContentReaderFactory _contentReaderFactory;
    private readonly IDataRepository _dataRepository;
    private readonly ILogger<CompileFileController> _logger;
    private readonly string _filePath;

    public CompileFileController(ILogger<CompileFileController> logger
                                            , IFileAnalyzer fileAnalyzer
                                            , IContentReaderFactory contentReaderFactory
                                            , IDataRepository dataRepository
                                            , IOptions<Settings> Settings)
    {
        _logger = logger;
        _fileAnalyzer = fileAnalyzer;
        _contentReaderFactory = contentReaderFactory;
        _dataRepository = dataRepository;
        _filePath = Settings.Value.MessageFilePath;
    }


    [HttpPost]
    public async Task<IActionResult> SaveMessages()
    {
        try
        {
            var contentReader = _contentReaderFactory.GetContentReader(_filePath);

            var fileContent = await contentReader.ReadAsync(_filePath);
            var messages = _fileAnalyzer.FindOriginalMessage(fileContent);
            var result = await _dataRepository.SaveMessagesAsync(messages);

            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {FilePath}", _filePath);
            return NotFound(new { error = "File not found", path = _filePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling file: {FilePath}", _filePath);
            return StatusCode(500, new { error = "An error occurred while processing the file" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = "")
    {
        try
        {
            var query = new MessageQuery
            {
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = search
            };

            var result = await _dataRepository.GetMessagesAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages");
            return StatusCode(500, new { error = "Failed to retrieve messages" });
        }
    }

}