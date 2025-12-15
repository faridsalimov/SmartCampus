using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Attendance
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CreateModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly ILessonService _lessonService;

        public CreateModel(
            IAttendanceService attendanceService,
            IStudentService studentService,
            ILessonService lessonService)
        {
            _attendanceService = attendanceService;
            _studentService = studentService;
            _lessonService = lessonService;
        }

        [BindProperty]
        public RecordAttendanceInput Input { get; set; } = new();

        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();
        public IList<LessonDto> Lessons { get; set; } = new List<LessonDto>();

        public class RecordAttendanceInput
        {
            public Guid StudentId { get; set; }
            public Guid LessonId { get; set; }
            public string? Status { get; set; }
        }

        public async Task OnGetAsync()
        {
            Students = (await _studentService.GetAllStudentsAsync()).ToList();
            Lessons = (await _lessonService.GetAllLessonsAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Students = (await _studentService.GetAllStudentsAsync()).ToList();
                Lessons = (await _lessonService.GetAllLessonsAsync()).ToList();
                return Page();
            }

            try
            {
                await _attendanceService.RecordAttendanceAsync(Input.StudentId, Input.LessonId, Input.Status ?? "Present");
                ToastHelper.ShowSuccess(this, "Attendance recorded successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error recording attendance: {ex.Message}");
                Students = (await _studentService.GetAllStudentsAsync()).ToList();
                Lessons = (await _lessonService.GetAllLessonsAsync()).ToList();
                return Page();
            }
        }
    }
}