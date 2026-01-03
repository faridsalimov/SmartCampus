using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMessageService _messageService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IMessageService messageService,
            IStudentService studentService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _studentService = studentService;
            _teacherService = teacherService;
            _userManager = userManager;
        }

        public Dictionary<string, (string ContactName, DateTime LastMessageDate, bool HasUnread, int UnreadCount, List<MessageDto> Messages, string? ContactProfilePhoto)> Conversations { get; set; }
            = new();

        public string? SelectedContactId { get; set; }
        public string? SelectedContactName { get; set; }
        public string? SelectedContactPhoto { get; set; }
        public List<MessageDto>? SelectedConversation { get; set; }

        public IList<(string Id, string Name, string Type)> AvailableContacts { get; set; }
            = new List<(string, string, string)>();

        public string? CurrentUserId { get; set; }
        public ApplicationUser? CurrentUser { get; set; }
        public int TotalUnreadCount { get; set; }
        public Dictionary<string, int> UnreadCountsByContact { get; set; } = new();

        public async Task OnGetAsync(string? contactId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            CurrentUserId = user.Id;
            CurrentUser = user;

            await LoadAvailableContacts();

            UnreadCountsByContact = await _messageService.GetUnreadCountsByContactAsync(CurrentUserId);
            TotalUnreadCount = UnreadCountsByContact.Values.Sum();

            var allMessages = await _messageService.GetAllMessagesAsync();
            var userMessages = allMessages
                .Where(m => (m.SenderId == CurrentUserId || m.ReceiverId == CurrentUserId) && !m.IsDeleted)
                .ToList();

            var conversationGroups = userMessages
                .GroupBy(m => m.SenderId == CurrentUserId ? m.ReceiverId : m.SenderId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(m => m.SentDate).ToList()
                );

            foreach (var contactIdKey in conversationGroups.Keys)
            {
                var messages = conversationGroups[contactIdKey];
                var lastMessage = messages.First();
                var contactName = lastMessage.SenderId == CurrentUserId ? lastMessage.ReceiverName : lastMessage.SenderName;
                var contactPhoto = lastMessage.SenderId == CurrentUserId ? lastMessage.ReceiverProfilePhoto : lastMessage.SenderProfilePhoto;
                var unreadCount = UnreadCountsByContact.ContainsKey(contactIdKey) ? UnreadCountsByContact[contactIdKey] : 0;
                var hasUnread = unreadCount > 0;

                Conversations[contactIdKey] = (contactName, lastMessage.SentDate, hasUnread, unreadCount, messages, contactPhoto);
            }

            if (!string.IsNullOrEmpty(contactId))
            {
                if (Conversations.ContainsKey(contactId))
                {
                    SelectedContactId = contactId;
                    var (name, _, _, unreadCount, messages, photo) = Conversations[contactId];
                    SelectedContactName = name;
                    SelectedContactPhoto = photo;
                    SelectedConversation = messages.OrderBy(m => m.SentDate).ToList();

                    await _messageService.MarkConversationAsReadAsync(CurrentUserId, contactId);
                    
                    UnreadCountsByContact[contactId] = 0;
                    TotalUnreadCount -= unreadCount;
                }
                else
                {
                    var contact = AvailableContacts.FirstOrDefault(c => c.Id == contactId);
                    if (contact != default)
                    {
                        SelectedContactId = contactId;
                        SelectedContactName = contact.Name;
                        SelectedConversation = new List<MessageDto>();
                    }
                }
            }
        }

        private async Task LoadAvailableContacts()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                var teachers = await _teacherService.GetAllTeachersAsync();

                var studentContacts = students
                    .Where(s => s.ApplicationUserId != CurrentUserId)
                    .Select(s => (s.ApplicationUserId ?? string.Empty, $"{s.FullName}", "Student"))
                    .Where(r => !string.IsNullOrEmpty(r.Item1))
                    .ToList();

                var teacherContacts = teachers
                    .Where(t => t.ApplicationUserId != CurrentUserId)
                    .Select(t => (t.ApplicationUserId ?? string.Empty, $"{t.FullName}", "Teacher"))
                    .Where(r => !string.IsNullOrEmpty(r.Item1))
                    .ToList();

                AvailableContacts = studentContacts
                    .Concat(teacherContacts)
                    .OrderBy(r => r.Item2)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostSendMessageAsync(string receiverId, string content)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                if (string.IsNullOrWhiteSpace(content))
                    return new JsonResult(new { success = false, message = "Message content cannot be empty." });

                var messageDto = await _messageService.SendMessageAsync(user.Id, receiverId, content);
                return new JsonResult(new { success = true, message = messageDto });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetUnreadCountAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var unreadCount = await _messageService.GetUnreadMessageCountAsync(user.Id);
                return new JsonResult(new { unreadCount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }
    }
}
