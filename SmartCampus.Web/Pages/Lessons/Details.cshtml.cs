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
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public DetailsModel(
            ILessonService lessonService,
            IAttendanceService attendanceService,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _lessonService = lessonService;
            _attendanceService = attendanceService;
            _studentService = studentService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        public LessonDto? Lesson { get; set; }
        public IList<StudentDto> EnrolledStudents { get; set; } = new List<StudentDto>();
        public IList<AttendanceDto> AttendanceRecords { get; set; } = new List<AttendanceDto>();
        public bool IsTeacher { get; set; }
        public bool IsOwner { get; set; }
        public bool CanManageAttendance { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Lesson = await _lessonService.GetLessonByIdAsync(id);

            if (Lesson == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            IsTeacher = userRoles.Contains("Teacher");

            if (IsTeacher && user != null)
            {
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (teacherId.HasValue)
                {
                    var isAuthorized = await _userContextHelper.IsTeacherTeachingGroupAsync(teacherId.Value, Lesson.GroupId);
                    IsOwner = Lesson.TeacherId == teacherId;

                    if (!isAuthorized && !IsOwner)
                        return Unauthorized();

                    CanManageAttendance = IsOwner || isAuthorized;
                }
            }
            else if (userRoles.Contains("Admin"))
            {
                CanManageAttendance = true;
            }

            try
            {
                EnrolledStudents = (await _studentService.GetStudentsByGroupAsync(Lesson.GroupId)).ToList();
                AttendanceRecords = (await _attendanceService.GetAttendanceByLessonAsync(id)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance data: {ex.Message}");
            }

            return Page();
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
                    return NotFound();

                var userRoles = await _userManager.GetRolesAsync(user);
                var isOwner = false;

                if (userRoles.Contains("Teacher"))
                {
                    var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                    if (teacherId.HasValue)
                    {
                        isOwner = lesson.TeacherId == teacherId.Value;
                    }
                }
                else if (!userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                if (!isOwner && !userRoles.Contains("Admin"))
                    return Unauthorized();

                await _attendanceService.RecordAttendanceAsync(studentId, lessonId, status);
                return new JsonResult(new { success = true, message = "Attendance updated successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (userRoles.Contains("Teacher") && user != null)
                {
                    var lesson = await _lessonService.GetLessonByIdAsync(id);
                    if (lesson != null)
                    {
                        var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                        if (teacherId.HasValue)
                        {
                            if (lesson.TeacherId != teacherId && !userRoles.Contains("Admin"))
                            {
                                return Unauthorized();
                            }
                        }
                    }
                }
                else if (!userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                await _lessonService.DeleteLessonAsync(id);
                ToastHelper.ShowSuccess(this, "Lesson deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting lesson: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}