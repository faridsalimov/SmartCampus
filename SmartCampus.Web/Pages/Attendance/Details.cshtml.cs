using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Attendance
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly ILessonService _lessonService;

        public DetailsModel(
            IAttendanceService attendanceService,
            IStudentService studentService,
            ILessonService lessonService)
        {
            _attendanceService = attendanceService;
            _studentService = studentService;
            _lessonService = lessonService;
        }

        public AttendanceDto? Attendance { get; set; }
        public StudentDto? Student { get; set; }
        public LessonDto? Lesson { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Attendance = await _attendanceService.GetAttendanceByIdAsync(id);

            if (Attendance == null)
            {
                return NotFound();
            }


            if (Attendance.StudentId != Guid.Empty)
            {
                Student = await _studentService.GetStudentByIdAsync(Attendance.StudentId);
            }

            if (Attendance.LessonId != Guid.Empty)
            {
                Lesson = await _lessonService.GetLessonByIdAsync(Attendance.LessonId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _attendanceService.DeleteAttendanceAsync(id);
                ToastHelper.ShowSuccess(this, "Attendance record deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting attendance: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}