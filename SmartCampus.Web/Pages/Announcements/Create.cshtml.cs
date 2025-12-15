using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Announcements
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CreateModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public CreateModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [BindProperty]
        public AnnouncementDto Announcement { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _announcementService.CreateAnnouncementAsync(Announcement);
                ToastHelper.ShowSuccess(this, "Announcement created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating announcement: {ex.Message}");
                return Page();
            }
        }
    }
}