using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Announcements
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IAnnouncementService announcementService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger)
        {
            _announcementService = announcementService;
            _teacherService = teacherService;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public AnnouncementDto Announcement { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Edit page loading for announcement: {AnnouncementId}", id);

                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);

                if (announcement == null)
                {
                    _logger.LogWarning("Announcement not found: {AnnouncementId}", id);
                    return NotFound();
                }

                Announcement = announcement;
                _logger.LogInformation("Announcement loaded: {Title}", announcement.Title);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcement for edit: {AnnouncementId}", id);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Edit submission started for announcement: {AnnouncementId}", Announcement?.Id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                        }
                    }
                    return Page();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found during edit");
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return Page();
                }

                // Verify the announcement exists (use untracked to avoid conflicts)
                var existingAnnouncement = await _announcementService.GetAnnouncementByIdAsNoTrackingAsync(Announcement.Id);
                if (existingAnnouncement == null)
                {
                    _logger.LogWarning("Announcement not found for update: {AnnouncementId}", Announcement.Id);
                    return NotFound();
                }

                // Preserve certain fields that shouldn't change
                Announcement.PublishedDate = existingAnnouncement.PublishedDate;
                Announcement.IsPublished = existingAnnouncement.IsPublished;
                Announcement.TeacherId = existingAnnouncement.TeacherId;

                _logger.LogInformation("Updating announcement: {AnnouncementId}", Announcement.Id);
                await _announcementService.UpdateAnnouncementAsync(Announcement);
                
                _logger.LogInformation("Announcement updated successfully: {AnnouncementId}", Announcement.Id);
                ToastHelper.ShowSuccess(this, "Announcement updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Error updating announcement: {ex.Message}");
                return Page();
            }
        }
    }
}