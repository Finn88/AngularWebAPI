using API.Dto;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message?> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessagesThread(string curentUsername, string recipientUsername);

        void DeleteMessage(Message message);
        void AddMessage(Message message);
        Task<bool> SaveAllAsync();
    }
}
