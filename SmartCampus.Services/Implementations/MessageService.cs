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