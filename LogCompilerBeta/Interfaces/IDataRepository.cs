using LogCompilerBeta.Entities.YourProjectName.Models;
using LogCompilerBeta.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IDataRepository
    {
        Task<bool> SaveMessagesAsync(List<string> messages);
        Task<MessageResult> GetMessagesAsync(MessageQuery query);
    }
}
