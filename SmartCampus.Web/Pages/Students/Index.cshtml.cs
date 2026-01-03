using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Students
{
    [Authorize(Roles = "Admin,Teacher")]
    public class IndexModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IStudentService studentService,
            ITeacherService teacherService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _groupService = groupService;
            _userManager = userManager;
        }

        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();
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

            IEnumerable<StudentDto> students = new List<StudentDto>();


            if (userRoles.Contains("Student"))
            {

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null)
                {
                    students = new List<StudentDto> { student };
                }
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    var teacherGroups = await _groupService.GetGroupsByTeacherAsync(teacher.Id);
                    var groupIds = teacherGroups.Select(g => g.Id).ToList();

                    var allStudents = await _studentService.GetAllStudentsAsync();
                    students = allStudents.Where(s => groupIds.Contains(s.GroupId)).ToList();
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                students = await _studentService.GetAllStudentsAsync();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                students = students.Where(s =>
                    (s.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Students = students.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin"))
                    return Unauthorized();

                await _studentService.DeleteStudentAsync(id);
                ToastHelper.ShowSuccess(this, "Student deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting student: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}