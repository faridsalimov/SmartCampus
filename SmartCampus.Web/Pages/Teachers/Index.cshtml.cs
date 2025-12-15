using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Teachers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class IndexModel : PageModel
    {
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _userManager = userManager;
        }

        public IList<TeacherDto> Teachers { get; set; } = new List<TeacherDto>();
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

            IEnumerable<TeacherDto> teachers = new List<TeacherDto>();


            if (userRoles.Contains("Student"))
            {

                return;
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    teachers = new List<TeacherDto> { teacher };
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                teachers = await _teacherService.GetAllTeachersAsync();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                teachers = teachers.Where(t =>
                    (t.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Teachers = teachers.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin"))
                    return Unauthorized();

                await _teacherService.DeleteTeacherAsync(id);
                ToastHelper.ShowSuccess(this, "Teacher deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting teacher: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}