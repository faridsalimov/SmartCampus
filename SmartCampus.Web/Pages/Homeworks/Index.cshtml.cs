using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Homework
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IHomeworkService homeworkService,
            IStudentService studentService,
            ITeacherService teacherService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager)
        {
            _homeworkService = homeworkService;
            _studentService = studentService;
            _teacherService = teacherService;
            _groupService = groupService;
            _userManager = userManager;
        }

        public IList<HomeworkDto> Homeworks { get; set; } = new List<HomeworkDto>();
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

            IEnumerable<HomeworkDto> homeworks = new List<HomeworkDto>();


            if (userRoles.Contains("Student"))
            {

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null)
                {
                    homeworks = await _homeworkService.GetHomeworkByStudentAsync(student.Id);
                }
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    homeworks = await _homeworkService.GetHomeworkByTeacherAsync(teacher.Id);
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                homeworks = await _homeworkService.GetAllHomeworkAsync();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                homeworks = homeworks.Where(h =>
                    (h.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Homeworks = homeworks.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin"))
                {
                    var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                    if (homework == null)
                        return NotFound();
                }

                await _homeworkService.DeleteHomeworkAsync(id);
                ToastHelper.ShowSuccess(this, "Homework deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting homework: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}