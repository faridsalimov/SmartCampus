using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Announcements
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public EditModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [BindProperty]
        public AnnouncementDto Announcement { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);

            if (announcement == null)
            {
                return NotFound();
            }

            Announcement = announcement;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _announcementService.UpdateAnnouncementAsync(Announcement);
                ToastHelper.ShowSuccess(this, "Announcement updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating announcement: {ex.Message}");
                return Page();
            }
        }
    }
}