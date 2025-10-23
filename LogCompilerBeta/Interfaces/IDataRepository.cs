using LogCompilerBeta.Entities.YourProjectName.Models;

namespace LogCompilerBeta.Interfaces
{
    public interface IDataRepository
    {
        Task<List<OriginalMessage>> SaveMessagesAsync(List<string> messages);
    }
}
