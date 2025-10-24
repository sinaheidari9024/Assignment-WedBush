using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CompileFileController : ControllerBase
{
    private readonly IFileAnalyzer _fileAnalyzer;
    private readonly IContentReader _contentReader;
    private readonly IDataRepository _dataRepository;
    private readonly ILogger<CompileFileController> _logger;

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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1024 * 1024 * 1024)] // 1GB
    [RequestFormLimits(MultipartBodyLengthLimit = 1024 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        var allowedExtensions = new[] { ".txt", ".log" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { error = "Only .txt and .log files are allowed" });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { error = "File size must be less than 10MB" });
        }

        try
        {
            var fileContent = await _contentReader.ReadInBatchesAsync(file);
            var messages = await _fileAnalyzer.FindOriginalMessageAsync(fileContent);
            var result = await _dataRepository.SaveMessagesAsync(messages);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded file: {FileName}", file.FileName);
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