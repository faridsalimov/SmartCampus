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
                // Send message via service
                var messageDto = await _messageService.SendMessageAsync(senderId, receiverId, content);

                // Get sender and receiver info
                var sender = await _userManager.FindByIdAsync(senderId);
                var receiver = await _userManager.FindByIdAsync(receiverId);

                // Message object for both clients
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

                // Send to receiver
                await Clients.User(receiverId).SendAsync("ReceiveMessage", messageObj);

                // Confirm to sender
                await Clients.Caller.SendAsync("MessageSent", messageObj);

                // Broadcast conversation update to both users
                var now = DateTime.Now;
                var preview = content.Length > 50 ? content.Substring(0, 50) + "..." : content;

                // Get unread count for receiver
                var unreadCountReceiver = await _messageService.GetUnreadMessageCountAsync(receiverId);

                // Update receiver's conversation list
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

                // Update sender's conversation list
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
                // Mark all messages as read
                var messages = await _messageService.GetConversationAsync(userId, senderId);
                var messagesToMark = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
                
                foreach (var msg in messagesToMark)
                {
                    await _messageService.MarkAsReadAsync(msg.Id);
                }

                // Get updated unread count for both users
                var totalUnreadCount = await _messageService.GetUnreadMessageCountAsync(userId);

                // Notify sender that messages are read with message IDs
                var messageIds = messagesToMark.Select(m => m.Id).ToList();
                await Clients.User(senderId).SendAsync("MessagesRead", new
                {
                    userId = userId,
                    messageIds = messageIds,
                    timestamp = DateTime.UtcNow,
                    totalUnreadCount = totalUnreadCount
                });

                // Update conversation UI for current user
                await Clients.Caller.SendAsync("UpdateConversationUnreadCount", new
                {
                    contactId = senderId,
                    unreadCount = 0
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error marking messages as read: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                // Notify others that user is online
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Notify others that user is offline
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
