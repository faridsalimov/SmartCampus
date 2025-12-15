using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId);
        Task<IEnumerable<Message>> GetByReceiverIdAsync(string receiverId);
        Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(string receiverId);
        Task<int> GetUnreadMessageCountAsync(string receiverId);
    }
}