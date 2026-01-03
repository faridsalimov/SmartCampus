using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Lessons
{
    [Authorize(Roles = "Teacher,Admin")]
    public class StartModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IAttendanceService _attendanceService;
        private readonly IGradeService _gradeService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public StartModel(
            ILessonService lessonService,
            IAttendanceService attendanceService,
            IGradeService gradeService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _lessonService = lessonService;
            _attendanceService = attendanceService;
            _gradeService = gradeService;
            _teacherService = teacherService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        public LessonSessionDto? SessionData { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsTeacherOwner { get; set; }

        [BindProperty]
        public List<GradeInput>? Grades { get; set; }

        public class GradeInput
        {
            public Guid StudentId { get; set; }
            public decimal Score { get; set; }
            public string? Feedback { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var lesson = await _lessonService.GetLessonByIdAsync(id);
                if (lesson == null)
                    return NotFound();

                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (!teacherId.HasValue)
                    return Unauthorized();

                IsTeacherOwner = lesson.TeacherId == teacherId.Value;
                if (!IsTeacherOwner && !User.IsInRole("Admin"))
                    return Unauthorized();

                if (lesson.IsCompleted)
                {
                    ToastHelper.ShowWarning(this, "This lesson has already been completed and finalized. Attendance cannot be modified.");
                    return RedirectToPage("Index");
                }

                if (lesson.IsActive)
                {
                    SessionData = await _attendanceService.GetActiveLessonSessionAsync(id, teacherId.Value);
                }
                else
                {
                    SessionData = await _attendanceService.StartLessonSessionAsync(id, teacherId.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAttendanceAsync(Guid lessonId, Guid studentId, string status)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
                if (lesson == null)
                    return new JsonResult(new { success = false, message = "Lesson not found." });

                if (lesson.IsCompleted)
                    return new JsonResult(new { success = false, message = "This lesson has already been completed. Attendance cannot be modified." });

                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (!teacherId.HasValue)
                    return Unauthorized();

                await _attendanceService.UpdateSessionAttendanceAsync(lessonId, studentId, status, teacherId.Value);

                var sessionData = await _attendanceService.GetActiveLessonSessionAsync(lessonId, teacherId.Value);
                return new JsonResult(new { success = true, session = sessionData });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEndSessionAsync(Guid lessonId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (!teacherId.HasValue)
                    return Unauthorized();

                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException("Lesson not found.");

                if (Grades != null && Grades.Any())
                {
                    foreach (var gradeInput in Grades.Where(g => g.Score >= 0 && g.Score <= 100))
                    {
                        var gradeDto = new GradeDto
                        {
                            StudentId = gradeInput.StudentId,
                            LessonId = lessonId,
                            GroupId = lesson.GroupId,
                            Score = gradeInput.Score,
                            Feedback = gradeInput.Feedback,
                            GradedDate = DateTime.UtcNow
                        };

                        await _gradeService.CreateGradeAsync(gradeDto);
                    }
                }

                await _attendanceService.EndLessonSessionAsync(lessonId, teacherId.Value);
                ToastHelper.ShowSuccess(this, "Lesson session ended successfully. Attendance and grades saved.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error ending lesson: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnGetSessionDataAsync(Guid lessonId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (!teacherId.HasValue)
                    return Unauthorized();

                var sessionData = await _attendanceService.GetActiveLessonSessionAsync(lessonId, teacherId.Value);
                return new JsonResult(sessionData);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
            }
        }
    }
}
