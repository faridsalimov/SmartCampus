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

        public class LessonVM
        {
            public Guid LessonId { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime LessonDate { get; set; }
            public string CourseName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string TeacherName { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
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



                var futuresLessons = lessons
                    .Where(l => l.LessonDate >= DateTime.Now.Date)
                    .OrderBy(l => l.LessonDate)
                    .ToList();


                if (!futuresLessons.Any())
                {
                    var sevenDaysAgo = DateTime.Now.AddDays(-7).Date;
                    futuresLessons = lessons
                        .Where(l => l.LessonDate >= sevenDaysAgo)
                        .OrderBy(l => l.LessonDate)
                        .Take(14)
                        .ToList();
                }


                if (!futuresLessons.Any())
                {
                    futuresLessons = lessons
                        .OrderByDescending(l => l.LessonDate)
                        .Take(14)
                        .OrderBy(l => l.LessonDate)
                        .ToList();
                }


                var groupedByDay = futuresLessons
                    .GroupBy(l => l.LessonDate.Date)
                    .OrderBy(g => g.Key)
                    .Take(7);

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