using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Announcements
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public DetailsModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public AnnouncementDto? Announcement { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Announcement = await _announcementService.GetAnnouncementByIdAsync(id);

            if (Announcement == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _announcementService.DeleteAnnouncementAsync(id);
                ToastHelper.ShowSuccess(this, "Announcement deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting announcement: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}