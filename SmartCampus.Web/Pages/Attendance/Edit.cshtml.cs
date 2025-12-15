using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Attendance
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly ILessonService _lessonService;

        public EditModel(
            IAttendanceService attendanceService,
            IStudentService studentService,
            ILessonService lessonService)
        {
            _attendanceService = attendanceService;
            _studentService = studentService;
            _lessonService = lessonService;
        }

        [BindProperty]
        public AttendanceDto Attendance { get; set; } = new();

        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();
        public IList<LessonDto> Lessons { get; set; } = new List<LessonDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);

            if (attendance == null)
            {
                return NotFound();
            }

            Attendance = attendance;
            Students = (await _studentService.GetAllStudentsAsync()).ToList();
            Lessons = (await _lessonService.GetAllLessonsAsync()).ToList();

            return Page();
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
                await _attendanceService.UpdateAttendanceAsync(Attendance);
                ToastHelper.ShowSuccess(this, "Attendance updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating attendance: {ex.Message}");
                Students = (await _studentService.GetAllStudentsAsync()).ToList();
                Lessons = (await _lessonService.GetAllLessonsAsync()).ToList();
                return Page();
            }
        }
    }
}