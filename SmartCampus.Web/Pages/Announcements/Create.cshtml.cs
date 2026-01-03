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
    public class CreateModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IAnnouncementService announcementService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger)
        {
            _announcementService = announcementService;
            _teacherService = teacherService;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public AnnouncementDto Announcement { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Announcement creation started. Title: {Title}", Announcement?.Title);
                
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
                    _logger.LogError("User not found");
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return Page();
                }

                _logger.LogInformation("User found: {UserId}", user.Id);

                Announcement.IsPublished = true;

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    Announcement.TeacherId = teacher.Id;
                }
                else
                {
                    _logger.LogInformation("No teacher record found for admin user: {UserId}", user.Id);
                }

                _logger.LogInformation("Creating announcement. Title: {Title}", Announcement.Title);

                await _announcementService.CreateAnnouncementAsync(Announcement);
                
                _logger.LogInformation("Announcement created successfully");
                ToastHelper.ShowSuccess(this, "Announcement created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Error creating announcement: {ex.Message}");
                return Page();
            }
        }
    }
}