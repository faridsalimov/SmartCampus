using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetAllMessagesAsync();
        Task<MessageDto?> GetMessageByIdAsync(Guid id);
        Task<IEnumerable<MessageDto>> GetSentMessagesAsync(string senderId);
        Task<IEnumerable<MessageDto>> GetReceivedMessagesAsync(string receiverId);
        Task<IEnumerable<MessageDto>> GetConversationAsync(string userId1, string userId2);
        Task<int> GetUnreadMessageCountAsync(string receiverId);
        Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content);
        Task MarkAsReadAsync(Guid messageId);
        Task DeleteMessageAsync(Guid id);
    }
}