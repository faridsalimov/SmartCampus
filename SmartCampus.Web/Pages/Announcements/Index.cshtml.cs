using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Announcements
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IAnnouncementService announcementService,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager)
        {
            _announcementService = announcementService;
            _studentService = studentService;
            _userManager = userManager;
        }

        public IList<AnnouncementDto> Announcements { get; set; } = new List<AnnouncementDto>();
        public string? SearchTerm { get; set; }
        public string? UserRole { get; set; }

        public async Task OnGetAsync(string? searchTerm = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var userRoles = await _userManager.GetRolesAsync(user);
            UserRole = userRoles.FirstOrDefault();
            SearchTerm = searchTerm;

            IEnumerable<AnnouncementDto> announcements = new List<AnnouncementDto>();

            if (userRoles.Contains("Student"))
            {
                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null)
                {
                    announcements = await _announcementService.GetAnnouncementsForStudentAsync(student.Id);
                }
                else
                {
                    announcements = await _announcementService.GetPublishedAnnouncementsAsync();
                }
            }
            else if (userRoles.Contains("Teacher"))
            {
                announcements = await _announcementService.GetPublishedAnnouncementsAsync();
            }
            else if (userRoles.Contains("Admin"))
            {
                announcements = await _announcementService.GetAllAnnouncementsAsync();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                announcements = announcements.Where(a =>
                    (a.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Content?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Announcements = announcements
                .OrderByDescending(a => a.PublishedDate)
                .ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);

                if (!userRoles.Contains("Admin"))
                {
                    var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                    if (announcement?.TeacherId.ToString() != user?.Id)
                        return Unauthorized();
                }

                await _announcementService.DeleteAnnouncementAsync(id);
                ToastHelper.ShowSuccess(this, "Announcement deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting announcement: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}