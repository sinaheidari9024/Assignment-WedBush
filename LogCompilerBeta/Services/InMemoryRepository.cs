using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Interfaces;
using System.Collections.Concurrent;

namespace LogCompilerBeta.Services
{
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

        public Task<List<OriginalMessage>> SaveMessagesAsync(List<string> messages)
        {
            if (messages == null || !messages.Any())
            {
                return Task.FromResult(new List<OriginalMessage>());
            }

            var originalMessages = new List<OriginalMessage>();

            foreach (var message in messages)
            {
                var id = Interlocked.Increment(ref _idCounter);
                var originalMessage = new OriginalMessage
                {
                    Id = id,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                };

                _messages.TryAdd(id, originalMessage);
                originalMessages.Add(originalMessage);
            }

            _logger.LogInformation("Data has been saved successfully in memory. Total messages: {Count}", _messages.Count);

            return Task.FromResult(originalMessages);
        }
    }
}
