using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Grades
{
    [Authorize(Roles = "Admin,Teacher")]
    public class EditModel : PageModel
    {
        private readonly IGradeService _gradeService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly UserContextHelper _userContextHelper;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IGradeService gradeService,
            IStudentService studentService,
            ICourseService courseService,
            IGroupService groupService,
            UserContextHelper userContextHelper,
            ILogger<EditModel> logger)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _courseService = courseService;
            _groupService = groupService;
            _userContextHelper = userContextHelper;
            _logger = logger;
        }

        [BindProperty]
        public GradeDto Grade { get; set; } = new();

        public IList<StudentDto> AvailableStudents { get; set; } = new List<StudentDto>();
        public IList<CourseDto> AvailableCourses { get; set; } = new List<CourseDto>();
        public IList<GroupDto> UserGroups { get; set; } = new List<GroupDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var grade = await _gradeService.GetGradeByIdAsync(id);
                if (grade == null)
                {
                    return NotFound();
                }


                var hasPermission = await VerifyGradeAccessAsync(userId, grade);
                if (!hasPermission)
                {
                    _logger.LogWarning($"User {userId} attempted unauthorized edit of grade {id}");
                    return Forbid();
                }

                Grade = grade;

                if (User.IsInRole("Admin"))
                {
                    AvailableStudents = (await _studentService.GetAllStudentsAsync()).ToList();
                    AvailableCourses = (await _courseService.GetAllCoursesAsync()).ToList();
                    UserGroups = (await _groupService.GetAllGroupsAsync()).ToList();
                }
                else if (User.IsInRole("Teacher"))
                {
                    var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);

                    var allStudents = await _studentService.GetAllStudentsAsync();
                    AvailableStudents = allStudents
                        .Where(s => teachingGroupIds.Contains(s.GroupId))
                        .ToList();

                    var allCourses = await _courseService.GetAllCoursesAsync();
                    AvailableCourses = allCourses
                        .Where(c => c.GroupId.HasValue && teachingGroupIds.Contains(c.GroupId.Value))
                        .ToList();

                    var allGroups = await _groupService.GetAllGroupsAsync();
                    UserGroups = allGroups
                        .Where(g => teachingGroupIds.Contains(g.Id))
                        .ToList();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading grade edit page");
                ToastHelper.ShowError(this, $"Error loading form: {ex.Message}");
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await ReloadFormDataAsync(userId);
                return Page();
            }

            try
            {

                var hasPermission = await VerifyGradeAccessAsync(userId, Grade);
                if (!hasPermission)
                {
                    _logger.LogWarning($"User {userId} attempted unauthorized update of grade {Grade.Id}");
                    ModelState.AddModelError(string.Empty, "You don't have permission to modify this grade.");
                    await ReloadFormDataAsync(userId);
                    return Page();
                }

                await _gradeService.UpdateGradeAsync(Grade);
                _logger.LogInformation($"User {userId} updated grade {Grade.Id}");
                ToastHelper.ShowSuccess(this, "Grade updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grade");
                ModelState.AddModelError(string.Empty, $"Error updating grade: {ex.Message}");
                await ReloadFormDataAsync(userId);
                return Page();
            }
        }

        private async Task<bool> VerifyGradeAccessAsync(string userId, GradeDto grade)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Teacher"))
            {
                if (!grade.GroupId.HasValue)
                    return false;

                var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);
                return teachingGroupIds.Contains(grade.GroupId.Value);
            }

            return false;
        }

        private async Task ReloadFormDataAsync(string userId)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    AvailableStudents = (await _studentService.GetAllStudentsAsync()).ToList();
                    AvailableCourses = (await _courseService.GetAllCoursesAsync()).ToList();
                    UserGroups = (await _groupService.GetAllGroupsAsync()).ToList();
                }
                else if (User.IsInRole("Teacher"))
                {
                    var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);

                    var allStudents = await _studentService.GetAllStudentsAsync();
                    AvailableStudents = allStudents
                        .Where(s => teachingGroupIds.Contains(s.GroupId))
                        .ToList();

                    var allCourses = await _courseService.GetAllCoursesAsync();
                    AvailableCourses = allCourses
                        .Where(c => c.GroupId.HasValue && teachingGroupIds.Contains(c.GroupId.Value))
                        .ToList();

                    var allGroups = await _groupService.GetAllGroupsAsync();
                    UserGroups = allGroups
                        .Where(g => teachingGroupIds.Contains(g.Id))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading form data");
            }
        }
    }
}