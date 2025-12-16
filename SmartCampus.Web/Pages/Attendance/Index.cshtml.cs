using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Attendance
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IAttendanceService attendanceService,
            IStudentService studentService,
            ITeacherService teacherService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager)
        {
            _attendanceService = attendanceService;
            _studentService = studentService;
            _teacherService = teacherService;
            _groupService = groupService;
            _userManager = userManager;
        }

        public IList<AttendanceDto> AttendanceRecords { get; set; } = new List<AttendanceDto>();
        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();
        public string? UserRole { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var userRoles = await _userManager.GetRolesAsync(user);
            UserRole = userRoles.FirstOrDefault();

            IEnumerable<AttendanceDto> records = new List<AttendanceDto>();


            if (userRoles.Contains("Student"))
            {

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null)
                {
                    records = await _attendanceService.GetAttendanceByStudentAsync(student.Id);
                    Students = new List<StudentDto> { student };
                }
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    var teacherGroups = await _groupService.GetGroupsByTeacherAsync(teacher.Id);
                    var teacherGroupIds = teacherGroups.Select(g => g.Id).ToList();

                    var allAttendance = await _attendanceService.GetAllAttendanceAsync();
                    var allStudents = await _studentService.GetAllStudentsAsync();

                    var groupStudents = allStudents
                        .Where(s => teacherGroupIds.Contains(s.GroupId))
                        .ToList();

                    records = allAttendance
                        .Where(a => groupStudents.Any(s => s.Id == a.StudentId))
                        .ToList();

                    Students = groupStudents;
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                records = await _attendanceService.GetAllAttendanceAsync();
                Students = (await _studentService.GetAllStudentsAsync()).ToList();
            }

            AttendanceRecords = records.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin"))
                    return Unauthorized();

                await _attendanceService.DeleteAttendanceAsync(id);
                ToastHelper.ShowSuccess(this, "Attendance record deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting attendance: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}