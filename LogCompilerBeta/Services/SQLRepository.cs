using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Infrastructure;
using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> SaveMessagesAsync(List<string> messages)
        {
            if (messages == null || !messages.Any())
            {
                return false;
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
                return true;
            }
            return false;
        }

        public async Task<List<OriginalMessage>> GetMessagesAsync()
        {
            return await _context.OriginalMessages.AsNoTracking().ToListAsync();
        }

        public async Task<MessageResult> GetMessagesAsync(MessageQuery query)
        {
            var messagesQuery = _context.OriginalMessages.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                messagesQuery = messagesQuery.Where(msg =>
                    msg.Message.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            var totalCount = await messagesQuery.CountAsync();

            var messages = await messagesQuery
                .OrderByDescending(msg => msg.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new MessageResult
            {
                Messages = messages,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

    }
}
