using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using SmartCampus.Services.Interfaces;
using SmartCampus.Data.Entities;

namespace SmartCampus.Web.Hubs
{
    [Authorize]
    public class MessagesHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesHub(
            IMessageService messageService,
            UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        public async Task SendMessage(string receiverId, string content)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(content?.Trim()))
                return;

            try
            {
                var messageDto = await _messageService.SendMessageAsync(senderId, receiverId, content);

                var sender = await _userManager.FindByIdAsync(senderId);
                var receiver = await _userManager.FindByIdAsync(receiverId);

                var messageObj = new
                {
                    messageDto.Id,
                    messageDto.SenderId,
                    messageDto.SenderName,
                    messageDto.SenderProfilePhoto,
                    messageDto.Content,
                    messageDto.SentDate,
                    messageDto.IsRead
                };

                await Clients.User(receiverId).SendAsync("ReceiveMessage", messageObj);

                await Clients.Caller.SendAsync("MessageSent", messageObj);

                var now = DateTime.Now;
                var preview = content.Length > 50 ? content.Substring(0, 50) + "..." : content;

                var unreadCountReceiver = await _messageService.GetUnreadMessageCountAsync(receiverId);

                await Clients.User(receiverId).SendAsync("UpdateConversation", new
                {
                    contactId = senderId,
                    contactName = sender?.FullName ?? "Unknown",
                    contactPhoto = sender?.ProfilePhoto,
                    lastMessage = content,
                    lastMessageDate = messageDto.SentDate,
                    preview = preview,
                    isSender = false,
                    unreadCount = unreadCountReceiver
                });

                await Clients.User(receiverId).SendAsync("NewMessageNotification", new
                {
                    senderId = senderId,
                    senderName = sender?.FullName ?? "Unknown",
                    senderProfilePhoto = sender?.ProfilePhoto,
                    messagePreview = preview,
                    totalUnreadCount = unreadCountReceiver
                });

                await Clients.Caller.SendAsync("UpdateConversation", new
                {
                    contactId = receiverId,
                    contactName = receiver?.FullName ?? "Unknown",
                    contactPhoto = receiver?.ProfilePhoto,
                    lastMessage = content,
                    lastMessageDate = messageDto.SentDate,
                    preview = preview,
                    isSender = true,
                    unreadCount = 0
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error sending message: {ex.Message}");
            }
        }

        public async Task MarkAsRead(string senderId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(senderId))
                return;

            try
            {
                var messages = await _messageService.GetConversationAsync(userId, senderId);
                var messagesToMark = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
                
                foreach (var msg in messagesToMark)
                {
                    await _messageService.MarkAsReadAsync(msg.Id);
                }

                var totalUnreadCount = await _messageService.GetUnreadMessageCountAsync(userId);

                var messageIds = messagesToMark.Select(m => m.Id).ToList();
                await Clients.User(senderId).SendAsync("MessagesRead", new
                {
                    userId = userId,
                    messageIds = messageIds,
                    timestamp = DateTime.UtcNow,
                    totalUnreadCount = totalUnreadCount
                });

                await Clients.Caller.SendAsync("UpdateConversationUnreadCount", new
                {
                    contactId = senderId,
                    unreadCount = 0
                });

                await Clients.User(userId).SendAsync("UnreadCountUpdated", new
                {
                    totalUnreadCount = totalUnreadCount
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error marking messages as read: {ex.Message}");
            }
        }

        public async Task GetUnreadCount()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                var totalUnreadCount = await _messageService.GetUnreadMessageCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCountUpdated", new
                {
                    totalUnreadCount = totalUnreadCount
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error getting unread count: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                await Clients.All.SendAsync("UserOnline", userId);

                var totalUnreadCount = await _messageService.GetUnreadMessageCountAsync(userId);

                await Clients.Caller.SendAsync("UnreadCountUpdated", new
                {
                    totalUnreadCount = totalUnreadCount
                });
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
