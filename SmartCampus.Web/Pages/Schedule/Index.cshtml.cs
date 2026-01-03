using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Web.Pages.Schedule
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ILessonService lessonService,
            IStudentService studentService,
            ITeacherService teacherService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager)
        {
            _lessonService = lessonService;
            _studentService = studentService;
            _teacherService = teacherService;
            _groupService = groupService;
            _userManager = userManager;
        }

        public IDictionary<DateTime, List<LessonVM>> GroupedLessons { get; set; } = new Dictionary<DateTime, List<LessonVM>>();
        public string? UserRole { get; set; }
        public DateTime CurrentMonth { get; set; }
        public DateTime Today { get; set; } = DateTime.Now;

        public class LessonVM
        {
            public Guid LessonId { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime LessonDate { get; set; }
            public string CourseName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string TeacherName { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(int? year = null, int? month = null)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;

            if (selectedMonth < 1) selectedMonth = 1;
            if (selectedMonth > 12) selectedMonth = 12;
            if (selectedYear < 2000) selectedYear = 2000;
            if (selectedYear > 2100) selectedYear = 2100;

            CurrentMonth = new DateTime(selectedYear, selectedMonth, 1);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var userRoles = await _userManager.GetRolesAsync(user);
            UserRole = userRoles.FirstOrDefault();

            try
            {
                IEnumerable<Core.DTOs.LessonDto> lessons = new List<Core.DTOs.LessonDto>();


                if (userRoles.Contains("Student"))
                {

                    var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                    if (student != null && student.GroupId != Guid.Empty)
                    {
                        lessons = await _lessonService.GetLessonsByGroupAsync(student.GroupId);
                    }
                }
                else if (userRoles.Contains("Teacher"))
                {

                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                    if (teacher != null)
                    {
                        lessons = await _lessonService.GetLessonsByTeacherAsync(teacher.Id);
                    }
                }
                else if (userRoles.Contains("Admin"))
                {

                    lessons = await _lessonService.GetAllLessonsAsync();
                }

                var monthStart = new DateTime(selectedYear, selectedMonth, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthLessons = lessons
                    .Where(l => l.LessonDate.Date >= monthStart && l.LessonDate.Date <= monthEnd)
                    .OrderBy(l => l.LessonDate)
                    .ToList();

                var groupedByDay = monthLessons
                    .GroupBy(l => l.LessonDate.Date)
                    .OrderBy(g => g.Key);

                foreach (var dayGroup in groupedByDay)
                {
                    var dayLessons = dayGroup
                        .Select(l => new LessonVM
                        {
                            LessonId = l.Id,
                            Title = l.Title,
                            LessonDate = l.LessonDate,
                            CourseName = l.CourseName ?? "Course",
                            Location = l.Location ?? "TBD",
                            TeacherName = l.TeacherName ?? string.Empty
                        })
                        .OrderBy(l => l.LessonDate.TimeOfDay)
                        .ToList();

                    GroupedLessons[dayGroup.Key] = dayLessons;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Schedule load error: {ex.Message}");
                GroupedLessons = new Dictionary<DateTime, List<LessonVM>>();
            }
        }
    }
}