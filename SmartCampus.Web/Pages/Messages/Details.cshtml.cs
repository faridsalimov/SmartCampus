using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Messages
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IMessageService _messageService;

        public DetailsModel(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public MessageDto? Message { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Message = await _messageService.GetMessageByIdAsync(id);

            if (Message == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _messageService.DeleteMessageAsync(id);
                ToastHelper.ShowSuccess(this, "Message deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting message: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}