using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDto>> GetAllMessagesAsync()
        {
            var messages = await _unitOfWork.MessageRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<MessageDto?> GetMessageByIdAsync(Guid id)
        {
            var message = await _unitOfWork.MessageRepository.GetByIdAsync(id);
            return _mapper.Map<MessageDto>(message);
        }

        public async Task<IEnumerable<MessageDto>> GetSentMessagesAsync(string senderId)
        {
            var messages = await _unitOfWork.MessageRepository.GetBySenderIdAsync(senderId);
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<IEnumerable<MessageDto>> GetReceivedMessagesAsync(string receiverId)
        {
            var messages = await _unitOfWork.MessageRepository.GetByReceiverIdAsync(receiverId);
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<IEnumerable<MessageDto>> GetConversationAsync(string userId1, string userId2)
        {
            var messages = await _unitOfWork.MessageRepository.GetConversationAsync(userId1, userId2);
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<int> GetUnreadMessageCountAsync(string receiverId)
        {
            return await _unitOfWork.MessageRepository.GetUnreadMessageCountAsync(receiverId);
        }

        public async Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(string receiverId)
        {
            var messages = await _unitOfWork.MessageRepository.GetByReceiverIdAsync(receiverId);
            var unreadMessages = messages.Where(m => !m.IsRead && !m.IsDeleted).ToList();
            return _mapper.Map<IEnumerable<MessageDto>>(unreadMessages);
        }

        public async Task<Dictionary<string, int>> GetUnreadCountsByContactAsync(string userId)
        {
            try
            {
                var receivedMessages = await _unitOfWork.MessageRepository.GetByReceiverIdAsync(userId);
                var unreadByContact = receivedMessages
                    .Where(m => !m.IsRead && !m.IsDeleted)
                    .GroupBy(m => m.SenderId)
                    .ToDictionary(g => g.Key, g => g.Count());
                return unreadByContact;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread counts by contact: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentDate = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MessageDto>(message);
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await _unitOfWork.MessageRepository.GetByIdAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                message.ReadDate = DateTime.UtcNow;
                _unitOfWork.MessageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task MarkConversationAsReadAsync(string userId, string contactId)
        {
            var messages = await _unitOfWork.MessageRepository.GetConversationAsync(userId, contactId);
            var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadDate = DateTime.UtcNow;
                _unitOfWork.MessageRepository.Update(message);
            }

            if (unreadMessages.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteMessageAsync(Guid id)
        {
            var message = await _unitOfWork.MessageRepository.GetByIdAsync(id);
            if (message != null)
            {
                message.IsDeleted = true;
                _unitOfWork.MessageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}