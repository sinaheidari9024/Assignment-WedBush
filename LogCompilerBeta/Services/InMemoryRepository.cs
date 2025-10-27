using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using System.Collections.Concurrent;

public class InMemoryRepository : IDataRepository
{
    private readonly ILogger<InMemoryRepository> _logger;
    private readonly ConcurrentDictionary<int, OriginalMessage> _messages;
    private int _idCounter;

    public InMemoryRepository(ILogger<InMemoryRepository> logger)
    {
        _logger = logger;
        _messages = new ConcurrentDictionary<int, OriginalMessage>();
        _idCounter = 0;
    }

    public async Task<bool> SaveMessagesAsync(List<string> messages)
    {
        if (messages == null || !messages.Any())
        {
            return false;
        }

        try
        {
            foreach (var message in messages)
            {
                _idCounter++;
                var originalMessage = new OriginalMessage
                {
                    Id = _idCounter,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                };
                _messages.TryAdd(_idCounter, originalMessage);
            }

            _logger.LogInformation("Saved {Count} messages", messages.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save messages");
            return false;
        }
    }

    public Task<MessageResult> GetMessagesAsync(MessageQuery query)
    {
        var allMessages = _messages.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            allMessages = allMessages.Where(msg =>
                msg.Message.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        var totalCount = allMessages.Count();

        var messages = allMessages
            .OrderByDescending(msg => msg.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new MessageResult
        {
            Messages = messages,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return Task.FromResult(result);
    }
}