using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Groups
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IGroupService groupService,
            IStudentService studentService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager)
        {
            _groupService = groupService;
            _studentService = studentService;
            _teacherService = teacherService;
            _userManager = userManager;
        }

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();
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

            IEnumerable<GroupDto> groups = new List<GroupDto>();


            if (userRoles.Contains("Student"))
            {

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null && student.GroupId != Guid.Empty)
                {
                    var studentGroup = await _groupService.GetGroupByIdAsync(student.GroupId);
                    if (studentGroup != null)
                        groups = new List<GroupDto> { studentGroup };
                }
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    groups = await _groupService.GetGroupsByTeacherAsync(teacher.Id);
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                groups = await _groupService.GetAllGroupsAsync();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                groups = groups.Where(g =>
                    (g.GroupName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.GroupCode?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Groups = groups.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin"))
                    return Unauthorized();

                await _groupService.DeleteGroupAsync(id);
                ToastHelper.ShowSuccess(this, "Group deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting group: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}