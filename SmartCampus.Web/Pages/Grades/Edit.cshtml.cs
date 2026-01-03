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
        private readonly ILessonService _lessonService;
        private readonly UserContextHelper _userContextHelper;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IGradeService gradeService,
            IStudentService studentService,
            ILessonService lessonService,
            UserContextHelper userContextHelper,
            ILogger<EditModel> logger)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _lessonService = lessonService;
            _userContextHelper = userContextHelper;
            _logger = logger;
        }

        [BindProperty]
        public GradeDto Grade { get; set; } = new();

        public IList<StudentDto> AvailableStudents { get; set; } = new List<StudentDto>();

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

                if (grade.GroupId.HasValue)
                {
                    var allStudents = await _studentService.GetAllStudentsAsync();
                    AvailableStudents = allStudents
                        .Where(s => s.GroupId == grade.GroupId)
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
                await LoadFormDataAsync();
                return Page();
            }

            try
            {
                var hasPermission = await VerifyGradeAccessAsync(userId, Grade);
                if (!hasPermission)
                {
                    _logger.LogWarning($"User {userId} attempted unauthorized update of grade {Grade.Id}");
                    ModelState.AddModelError(string.Empty, "You don't have permission to modify this grade.");
                    await LoadFormDataAsync();
                    return Page();
                }

                Grade.LetterGrade = GetLetterGrade(Grade.Score);

                await _gradeService.UpdateGradeAsync(Grade);
                _logger.LogInformation($"User {userId} updated grade {Grade.Id}");
                ToastHelper.ShowSuccess(this, "Grade updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grade");
                ModelState.AddModelError(string.Empty, $"Error updating grade: {ex.Message}");
                await LoadFormDataAsync();
                return Page();
            }
        }

        private async Task<bool> VerifyGradeAccessAsync(string userId, GradeDto grade)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Teacher"))
            {
                var lesson = await _lessonService.GetLessonByIdAsync(grade.LessonId);
                if (lesson == null)
                    return false;

                return lesson.TeacherId.ToString() == userId || (await _userContextHelper.IsTeacherTeachingGroupAsync(Guid.Parse(userId), lesson.GroupId));
            }

            return false;
        }

        private async Task LoadFormDataAsync()
        {
            try
            {
                if (Grade.GroupId.HasValue)
                {
                    var allStudents = await _studentService.GetAllStudentsAsync();
                    AvailableStudents = allStudents
                        .Where(s => s.GroupId == Grade.GroupId)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form data");
            }
        }

        private string GetLetterGrade(decimal score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}