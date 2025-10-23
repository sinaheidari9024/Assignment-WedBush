using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Infrastructure;
using LogCompilerBeta.Interfaces;

namespace LogCompilerBeta.Services
{
    public class SQLRepository : IDataRepository
    {
        private readonly ILogger<SQLRepository> _logger;
        private readonly ApplicationDbContext _context;

        public SQLRepository(
        ILogger<SQLRepository> logger,
        ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<OriginalMessage>> SaveMessagesAsync(List<string> messages)
        {
            if (messages == null || !messages.Any())
            {
                return new List<OriginalMessage>();
            }

            var originalMessages = messages.Select(message => new OriginalMessage
            {
                Message = message,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.OriginalMessages.AddRangeAsync(originalMessages);
            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                _logger.LogInformation("Data has been saved successfully.");
            }
            return originalMessages;
        }

    }
}
