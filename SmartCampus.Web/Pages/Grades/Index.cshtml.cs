using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Grades
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IGradeService _gradeService;
        private readonly IStudentService _studentService;
        private readonly UserContextHelper _userContextHelper;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IGradeService gradeService,
            IStudentService studentService,
            UserContextHelper userContextHelper,
            ILogger<IndexModel> logger)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _userContextHelper = userContextHelper;
            _logger = logger;
        }

        public IList<GradeDto> Grades { get; set; } = new List<GradeDto>();
        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();
        public Guid? FilterStudentId { get; set; }
        public string? SearchTerm { get; set; }
        public string UserRole { get; set; } = string.Empty;

        public async Task OnGetAsync(string? searchTerm = null, Guid? filterStudentId = null)
        {
            SearchTerm = searchTerm;
            FilterStudentId = filterStudentId;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                Grades = new List<GradeDto>();
                return;
            }

            try
            {

                if (User.IsInRole("Student"))
                {
                    await LoadStudentGradesAsync(userId);
                    UserRole = "Student";
                }
                else if (User.IsInRole("Teacher"))
                {
                    await LoadTeacherGradesAsync(userId);
                    UserRole = "Teacher";
                }
                else if (User.IsInRole("Admin"))
                {
                    await LoadAdminGradesAsync();
                    UserRole = "Admin";

                    Students = (await _studentService.GetAllStudentsAsync()).ToList();
                }


                if (!string.IsNullOrEmpty(searchTerm))
                {
                    Grades = Grades.Where(g =>
                        (g.StudentName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }


                if (filterStudentId.HasValue && filterStudentId != Guid.Empty)
                {

                    var hasPermission = await VerifyStudentAccessAsync(userId, filterStudentId.Value);
                    if (hasPermission)
                    {
                        Grades = Grades.Where(g => g.StudentId == filterStudentId).ToList();
                    }
                    else
                    {
                        _logger.LogWarning($"User {userId} attempted unauthorized access to student {filterStudentId}");
                        ToastHelper.ShowError(this, "You don't have permission to view this student's grades.");
                        Grades = new List<GradeDto>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading grades for user {userId}");
                ViewData["ErrorMessage"] = $"Error loading grades: {ex.Message}";
                Grades = new List<GradeDto>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {

                var grade = await _gradeService.GetGradeByIdAsync(id);
                if (grade == null)
                {
                    ToastHelper.ShowError(this, "Grade not found.");
                    return RedirectToPage();
                }


                var hasPermission = await VerifyGradeModificationAsync(userId, grade);
                if (!hasPermission)
                {
                    _logger.LogWarning($"User {userId} attempted unauthorized deletion of grade {id}");
                    ToastHelper.ShowError(this, "You don't have permission to delete this grade.");
                    return RedirectToPage();
                }

                await _gradeService.DeleteGradeAsync(id);
                _logger.LogInformation($"User {userId} deleted grade {id}");
                ToastHelper.ShowSuccess(this, "Grade deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting grade {id}");
                ToastHelper.ShowError(this, $"Error deleting grade: {ex.Message}");
            }

            return RedirectToPage();
        }

        private async Task LoadStudentGradesAsync(string userId)
        {
            var student = await _studentService.GetStudentByApplicationUserIdAsync(userId);
            if (student == null)
            {
                _logger.LogWarning($"Student record not found for user {userId}");
                Grades = new List<GradeDto>();
                return;
            }


            Grades = (await _gradeService.GetGradesByStudentAsync(student.Id)).ToList();
        }

        private async Task LoadTeacherGradesAsync(string userId)
        {

            var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);
            if (!teachingGroupIds.Any())
            {
                Grades = new List<GradeDto>();
                return;
            }


            var allGrades = new List<GradeDto>();
            foreach (var groupId in teachingGroupIds)
            {
                var groupGrades = await _gradeService.GetGradesByGroupAsync(groupId);
                allGrades.AddRange(groupGrades);
            }

            Grades = allGrades.ToList();
        }

        private async Task LoadAdminGradesAsync()
        {

            Grades = (await _gradeService.GetAllGradesAsync()).ToList();
        }

        private async Task<bool> VerifyStudentAccessAsync(string userId, Guid studentId)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Student"))
            {
                var student = await _studentService.GetStudentByApplicationUserIdAsync(userId);
                return student?.Id == studentId;
            }

            if (User.IsInRole("Teacher"))
            {
                var targetStudent = await _studentService.GetStudentByIdAsync(studentId);
                if (targetStudent == null)
                    return false;

                var teachingGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(userId);
                return teachingGroupIds.Contains(targetStudent.GroupId);
            }

            return false;
        }

        private async Task<bool> VerifyGradeModificationAsync(string userId, GradeDto grade)
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
    }
}