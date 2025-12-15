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
    public class CreateModel : PageModel
    {
        private readonly IGradeService _gradeService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly UserContextHelper _userContextHelper;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IGradeService gradeService,
            IStudentService studentService,
            ICourseService courseService,
            IGroupService groupService,
            UserContextHelper userContextHelper,
            ILogger<CreateModel> logger)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _courseService = courseService;
            _groupService = groupService;
            _userContextHelper = userContextHelper;
            _logger = logger;
        }

        [BindProperty]
        public GradeInputModel Input { get; set; } = new();

        public IList<StudentDto> AvailableStudents { get; set; } = new List<StudentDto>();
        public IList<CourseDto> AvailableCourses { get; set; } = new List<CourseDto>();
        public IList<GroupDto> UserGroups { get; set; } = new List<GroupDto>();

        public class GradeInputModel
        {
            public Guid StudentId { get; set; }
            public Guid CourseId { get; set; }
            public Guid GroupId { get; set; }
            public decimal Score { get; set; }
            public string? Feedback { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

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
                    if (!teachingGroupIds.Any())
                    {
                        ToastHelper.ShowError(this, "You are not assigned to any groups.");
                        return RedirectToPage("Index");
                    }


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
                _logger.LogError(ex, "Error loading grade creation page");
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


            if (Input.StudentId == Guid.Empty)
            {
                ModelState.AddModelError("Input.StudentId", "Please select a student.");
            }


            if (Input.GroupId == Guid.Empty)
            {
                ModelState.AddModelError("Input.GroupId", "Please select a group.");
            }


            if (Input.CourseId == Guid.Empty)
            {
                ModelState.AddModelError("Input.CourseId", "Please select a course.");
            }


            if (Input.Score < 0 || Input.Score > 100)
            {
                ModelState.AddModelError("Input.Score", "Score must be between 0 and 100.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadFormDataAsync(userId);
                return Page();
            }

            try
            {

                var hasPermission = await VerifyGroupAccessAsync(userId, Input.GroupId);
                if (!hasPermission)
                {
                    _logger.LogWarning($"User {userId} attempted to create grade for unauthorized group {Input.GroupId}");
                    ModelState.AddModelError(string.Empty, "You don't have permission to create grades for this group.");
                    await ReloadFormDataAsync(userId);
                    return Page();
                }


                var student = await _studentService.GetStudentByIdAsync(Input.StudentId);
                if (student?.GroupId != Input.GroupId)
                {
                    ModelState.AddModelError("Input.StudentId", "The selected student is not in the specified group.");
                    await ReloadFormDataAsync(userId);
                    return Page();
                }


                var gradeDto = new GradeDto
                {
                    StudentId = Input.StudentId,
                    CourseId = Input.CourseId != Guid.Empty ? Input.CourseId : null,
                    GroupId = Input.GroupId,
                    Score = Input.Score,
                    Feedback = Input.Feedback,
                    GradeType = "Manual",
                    GradedDate = DateTime.UtcNow
                };

                await _gradeService.CreateGradeAsync(gradeDto);

                _logger.LogInformation($"User {userId} created grade for student {Input.StudentId} in group {Input.GroupId}");
                ToastHelper.ShowSuccess(this, "Grade recorded successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating grade");
                ModelState.AddModelError(string.Empty, $"Error recording grade: {ex.Message}");
                await ReloadFormDataAsync(userId);
                return Page();
            }
        }

        private async Task<bool> VerifyGroupAccessAsync(string userId, Guid groupId)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Teacher"))
            {
                var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);
                return teachingGroupIds.Contains(groupId);
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