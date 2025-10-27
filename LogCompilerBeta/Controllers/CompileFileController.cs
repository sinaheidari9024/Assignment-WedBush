using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Interfaces.Factory;
using LogCompilerBeta.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CompileFileController : ControllerBase
{
    private readonly IFileAnalyzer _fileAnalyzer;
    private readonly IContentReaderFactory _contentReaderFactory;
    private readonly IDataRepository _dataRepository;
    private readonly ILogger<CompileFileController> _logger;

    public CompileFileController(ILogger<CompileFileController> logger
                                            , IFileAnalyzer fileAnalyzer
                                            , IContentReaderFactory contentReaderFactory
                                            , IDataRepository dataRepository)
    {
        _logger = logger;
        _fileAnalyzer = fileAnalyzer;
        _contentReaderFactory = contentReaderFactory;
        _dataRepository = dataRepository;
    }

    private readonly string filePath = "C:\\Assignment\\AVATAR3.messages.log";


    [HttpPost]
    public async Task<IActionResult> SaveMessages()
    {
        try
        {
            var contentReader = _contentReaderFactory.GetContentReader(filePath);

            var fileContent = await contentReader.ReadAsync(filePath);
            var messages = _fileAnalyzer.FindOriginalMessage(fileContent);
            var result = await _dataRepository.SaveMessagesAsync(messages);

            return Ok(result);
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