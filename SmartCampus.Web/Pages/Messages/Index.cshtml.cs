using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IMessageService messageService,
            UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        public IList<MessageDto> Messages { get; set; } = new List<MessageDto>();
        public string? SearchTerm { get; set; }
        public string? UserRole { get; set; }
        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync(string? searchTerm = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            CurrentUserId = user.Id;
            var userRoles = await _userManager.GetRolesAsync(user);
            UserRole = userRoles.FirstOrDefault();
            SearchTerm = searchTerm;

            IEnumerable<MessageDto> messages = await _messageService.GetAllMessagesAsync();


            if (userRoles.Contains("Admin"))
            {


            }
            else
            {

                messages = messages.Where(m =>
                    m.SenderId == CurrentUserId ||
                    m.ReceiverId == CurrentUserId
                ).ToList();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                messages = messages.Where(m =>
                    (m.Content?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Messages = messages.OrderByDescending(m => m.SentDate).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var message = await _messageService.GetMessageByIdAsync(id);
                if (message == null)
                    return NotFound();

                var userRoles = await _userManager.GetRolesAsync(user);


                if (message.SenderId != user.Id && !userRoles.Contains("Admin"))
                    return Unauthorized();

                await _messageService.DeleteMessageAsync(id);
                ToastHelper.ShowSuccess(this, "Message deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting message: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}