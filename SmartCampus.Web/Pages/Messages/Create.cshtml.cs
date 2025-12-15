using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Messages
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IMessageService _messageService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public CreateModel(
            IMessageService messageService,
            IStudentService studentService,
            ITeacherService teacherService)
        {
            _messageService = messageService;
            _studentService = studentService;
            _teacherService = teacherService;
        }

        [BindProperty]
        public SendMessageInput Input { get; set; } = new();

        public IList<(string Id, string Name, string Type)> Recipients { get; set; }
            = new List<(string, string, string)>();

        public class SendMessageInput
        {
            public string? ReceiverId { get; set; }
            public string? SenderName { get; set; }
            public string? Subject { get; set; }
            public string? Content { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                var teachers = await _teacherService.GetAllTeachersAsync();

                var studentRecipients = students
                    .Select(s => (s.Id.ToString(), $"{s.FullName} (Student)", "Student"))
                    .ToList();

                var teacherRecipients = teachers
                    .Select(t => (t.Id.ToString(), $"{t.FullName} (Teacher)", "Teacher"))
                    .ToList();

                Recipients = studentRecipients
                    .Concat(teacherRecipients)
                    .OrderBy(r => r.Item2)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipients: {ex.Message}");
                Recipients = new List<(string, string, string)>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(senderId))
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to send message: user not authenticated.");
                    await OnGetAsync();
                    return Page();
                }

                if (string.IsNullOrEmpty(Input.ReceiverId))
                {
                    ModelState.AddModelError("Input.ReceiverId",
                        "Please select a recipient.");
                    await OnGetAsync();
                    return Page();
                }

                if (string.IsNullOrEmpty(Input.Content))
                {
                    ModelState.AddModelError("Input.Content",
                        "Message content cannot be empty.");
                    await OnGetAsync();
                    return Page();
                }

                await _messageService.SendMessageAsync(
                    senderId,
                    Input.ReceiverId,
                    Input.Content);

                ToastHelper.ShowSuccess(this, "Message sent successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error sending message: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}