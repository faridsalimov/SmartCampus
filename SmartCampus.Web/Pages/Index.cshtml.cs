using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.ViewModels;

namespace SmartCampus.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IHomeworkService _homeworkService;
        private readonly IGradeService _gradeService;
        private readonly IAnnouncementService _announcementService;
        private readonly IAttendanceService _attendanceService;
        private readonly IGroupService _groupService;
        private readonly ILessonService _lessonService;
        private readonly IScheduleService _scheduleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ICourseService courseService,
            IStudentService studentService,
            ITeacherService teacherService,
            IHomeworkService homeworkService,
            IGradeService gradeService,
            IAnnouncementService announcementService,
            IAttendanceService attendanceService,
            IGroupService groupService,
            ILessonService lessonService,
            IScheduleService scheduleService,
            UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _studentService = studentService;
            _teacherService = teacherService;
            _homeworkService = homeworkService;
            _gradeService = gradeService;
            _announcementService = announcementService;
            _attendanceService = attendanceService;
            _groupService = groupService;
            _lessonService = lessonService;
            _scheduleService = scheduleService;
            _userManager = userManager;
        }


        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalHomeworks { get; set; }
        public int TotalAnnouncements { get; set; }
        public int TotalGroups { get; set; }
        public int TotalLessons { get; set; }


        public IList<dynamic> RecentAnnouncements { get; set; } = new List<dynamic>();
        public IList<dynamic> UpcomingHomework { get; set; } = new List<dynamic>();
        public IList<dynamic> RecentGrades { get; set; } = new List<dynamic>();
        public IList<dynamic> UpcomingLessons { get; set; } = new List<dynamic>();


        public decimal StudentGPA { get; set; }
        public decimal StudentAttendance { get; set; }
        public int StudentCoursesEnrolled { get; set; }
        public IList<dynamic> StudentCourses { get; set; } = new List<dynamic>();


        public int TeacherCoursesTeaching { get; set; }
        public int TeacherTotalStudents { get; set; }
        public IList<dynamic> TeacherCourses { get; set; } = new List<dynamic>();
        public IList<dynamic> TeacherSchedule { get; set; } = new List<dynamic>();


        public StudentDashboardViewModel? StudentDashboard { get; set; }
        public TeacherDashboardViewModel? TeacherDashboard { get; set; }
        public AdminDashboardViewModel? AdminDashboard { get; set; }


        public string CoursesChartData { get; set; } = "{}";
        public string EnrollmentChartData { get; set; } = "{}";
        public string GradesChartData { get; set; } = "{}";
        public string AttendanceChartData { get; set; } = "{}";
        public string ActivityChartData { get; set; } = "{}";


        public string? CurrentUserFullName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    CurrentUserFullName = user.FullName ?? user.UserName ?? "User";
                }


                await LoadGlobalStatistics();


                await LoadActivityFeeds();


                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains("Student"))
                    {
                        await LoadStudentData(user.Id);
                        await LoadStudentDashboard(user.Id);
                    }
                    else if (userRoles.Contains("Teacher"))
                    {
                        await LoadTeacherData(user.Id);
                        await LoadTeacherDashboard(user.Id);
                    }
                    else if (userRoles.Contains("Admin"))
                    {
                        await LoadAdminDashboard();
                    }
                }


                await PrepareChartDataAsync();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Home load error: {ex.Message}");
            }

            return Page();
        }

        private async Task LoadGlobalStatistics()
        {
            TotalCourses = (await _courseService.GetAllCoursesAsync()).Count();
            TotalStudents = (await _studentService.GetAllStudentsAsync()).Count();
            TotalTeachers = (await _teacherService.GetAllTeachersAsync()).Count();
            TotalHomeworks = (await _homeworkService.GetAllHomeworkAsync()).Count();
            TotalAnnouncements = (await _announcementService.GetAllAnnouncementsAsync()).Count();
            TotalGroups = (await _groupService.GetAllGroupsAsync()).Count();
            TotalLessons = (await _lessonService.GetAllLessonsAsync()).Count();
        }

        private async Task LoadActivityFeeds()
        {

            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            RecentAnnouncements = announcements
                .OrderByDescending(a => a.PublishedDate)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Content,
                    PublishedDate = a.PublishedDate.ToString("MMM dd, yyyy HH:mm"),
                    TimeSince = GetTimeSince(a.PublishedDate)
                })
                .Cast<dynamic>()
                .ToList();


            var homework = await _homeworkService.GetAllHomeworkAsync();
            UpcomingHomework = homework
                .OrderBy(h => h.DueDate)
                .Take(5)
                .Select(h => new
                {
                    h.Id,
                    h.Title,
                    DueDate = h.DueDate.ToString("MMM dd, yyyy"),
                    h.Type,
                    DaysLeft = (h.DueDate - DateTime.Now).Days
                })
                .Cast<dynamic>()
                .ToList();


            var grades = await _gradeService.GetAllGradesAsync();
            RecentGrades = grades
                .OrderByDescending(g => g.GradedDate)
                .Take(5)
                .Select(g => new
                {
                    g.Id,
                    Score = g.Score.ToString("F1"),
                    g.LetterGrade,
                    GradedDate = g.GradedDate.ToString("MMM dd, yyyy")
                })
                .Cast<dynamic>()
                .ToList();


            var lessons = await _lessonService.GetAllLessonsAsync();
            UpcomingLessons = lessons
                .OrderBy(l => l.LessonDate)
                .Take(5)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    LessonDate = l.LessonDate.ToString("MMM dd, yyyy HH:mm"),
                    l.Location,
                    l.Content
                })
                .Cast<dynamic>()
                .ToList();
        }

        private async Task LoadStudentData(string userId)
        {

            var student = await _studentService.GetStudentByApplicationUserIdAsync(userId);

            if (student != null)
            {

                var studentGrades = await _gradeService.GetGradesByStudentAsync(student.Id);
                var gradesList = studentGrades.ToList();
                StudentGPA = gradesList.Count > 0
                    ? (decimal)gradesList.Average(g => (double)g.Score)
                    : 0;


                var allAttendance = await _attendanceService.GetAllAttendanceAsync();
                var attendance = allAttendance.ToList();
                StudentAttendance = attendance.Count > 0
                    ? (decimal)(attendance.Count(a => a.Status == "Present") * 100.0 / attendance.Count())
                    : 0;


                var courses = await _courseService.GetAllCoursesAsync();
                var activeCourses = courses.Where(c => c.IsActive).ToList();
                StudentCoursesEnrolled = activeCourses.Count;
                StudentCourses = activeCourses
                    .Take(6)
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Code,
                        c.Credits
                    })
                    .Cast<dynamic>()
                    .ToList();
            }
        }

        private async Task LoadTeacherData(string userId)
        {

            var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(userId);

            if (teacher != null)
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var teacherCourses = courses.Where(c => c.TeacherId == teacher.Id).ToList();

                TeacherCoursesTeaching = teacherCourses.Count;


                TeacherTotalStudents = await _teacherService.GetTotalStudentsForTeacherAsync(teacher.Id);

                TeacherCourses = teacherCourses
                    .Take(6)
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Code,
                        c.Credits,
                        c.Semester
                    })
                    .Cast<dynamic>()
                    .ToList();


                var schedules = await _scheduleService.GetSchedulesByTeacherAsync(teacher.Id);
                TeacherSchedule = schedules
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.StartTime)
                    .Take(10)
                    .Select(s => new
                    {
                        Day = s.DayOfWeek,
                        Time = $"{s.StartTime:h:mm tt} - {s.EndTime:h:mm tt}",
                        Course = s.CourseName ?? "N/A",
                        Room = s.Location
                    })
                    .Cast<dynamic>()
                    .ToList();
            }
        }


        private async Task LoadStudentDashboard(string userId)
        {
            try
            {
                var student = await _studentService.GetStudentByApplicationUserIdAsync(userId);
                if (student == null)
                    return;

                StudentDashboard = new StudentDashboardViewModel();


                var studentGroup = await _groupService.GetStudentGroupAsync(student.Id);
                if (studentGroup != null)
                {
                    StudentDashboard.GroupOverview.GroupId = studentGroup.Id;
                    StudentDashboard.GroupOverview.GroupName = studentGroup.GroupName ?? "Group";
                    StudentDashboard.GroupOverview.GroupCode = studentGroup.GroupCode ?? "N/A";
                    StudentDashboard.GroupOverview.StudentCount = studentGroup.StudentCount;

                    var groupmates = await _studentService.GetStudentsByGroupAsync(studentGroup.Id);
                    StudentDashboard.GroupOverview.Groupmates = groupmates
                        .Where(s => s.Id != student.Id)
                        .Take(10)
                        .Select(s => new StudentSummaryVM
                        {
                            StudentId = s.Id,
                            FullName = s.FullName,
                            StudentId_Number = s.StudentId
                        })
                        .ToList();
                }


                var studentGrades = await _gradeService.GetGradesByStudentAsync(student.Id);
                var gradesList = studentGrades.ToList();
                StudentDashboard.CurrentGPA = gradesList.Count > 0
                    ? (decimal)gradesList.Average(g => (double)g.Score)
                    : 0;

                StudentDashboard.AttendancePercentage = await _attendanceService.GetStudentAttendancePercentageAsync(student.Id);

                var courses = await _courseService.GetAllCoursesAsync();
                StudentDashboard.EnrolledCoursesCount = courses.Count(c => c.IsActive);


                if (studentGroup != null)
                {
                    var upcomingLessons = await _lessonService.GetLessonsByGroupAsync(studentGroup.Id);


                    var futureList = upcomingLessons
                        .Where(l => l.LessonDate >= DateTime.Now.Date)
                        .OrderBy(l => l.LessonDate)
                        .ToList();


                    if (!futureList.Any())
                    {
                        var sevenDaysAgo = DateTime.Now.AddDays(-7).Date;
                        futureList = upcomingLessons
                            .Where(l => l.LessonDate >= sevenDaysAgo)
                            .OrderBy(l => l.LessonDate)
                            .ToList();
                    }


                    if (!futureList.Any())
                    {
                        futureList = upcomingLessons
                            .OrderByDescending(l => l.LessonDate)
                            .Take(5)
                            .OrderBy(l => l.LessonDate)
                            .ToList();
                    }

                    StudentDashboard.UpcomingLessons = futureList
                        .Take(5)
                        .Select(l => new LessonOverviewVM
                        {
                            LessonId = l.Id,
                            Title = l.Title,
                            LessonDate = l.LessonDate,
                            Location = l.Location ?? "TBD",
                            CourseName = l.CourseName ?? "Course"
                        })
                        .ToList();
                }


                var allHomework = await _homeworkService.GetAllHomeworkAsync();
                StudentDashboard.Homeworks = allHomework
                    .OrderBy(h => h.DueDate)
                    .Select(h => new HomeworkSummaryVM
                    {
                        HomeworkId = h.Id,
                        Title = h.Title,
                        DueDate = h.DueDate,
                        Status = h.DueDate < DateTime.Now ? "Overdue" : "Pending",
                        DaysUntilDue = (h.DueDate - DateTime.Now).Days,
                        Type = h.Type
                    })
                    .ToList();

                StudentDashboard.PendingHomeworkCount = StudentDashboard.Homeworks.Count(h => h.Status == "Pending");
                StudentDashboard.OverdueHomeworkCount = StudentDashboard.Homeworks.Count(h => h.Status == "Overdue");
                StudentDashboard.CompletedHomeworkCount = allHomework.Count(h => h.DueDate <= DateTime.Now);


                StudentDashboard.TotalClassesAttended = await _attendanceService.GetPresentCountForStudentAsync(student.Id);
                StudentDashboard.TotalClassesMissed = await _attendanceService.GetAbsentCountForStudentAsync(student.Id);

                var recentAttendance = await _attendanceService.GetAttendanceByStudentAsync(student.Id);
                StudentDashboard.RecentAttendance = recentAttendance
                    .OrderByDescending(a => a.AttendanceDate)
                    .Take(15)
                    .Select(a => new AttendanceRecordVM
                    {
                        AttendanceId = a.Id,
                        Date = a.AttendanceDate,
                        Status = a.Status,
                        LessonTitle = "Class"
                    })
                    .ToList();


                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var attendance30Days = recentAttendance
                    .Where(a => a.AttendanceDate >= thirtyDaysAgo)
                    .ToList();

                StudentDashboard.TotalClassesAttendedLast30Days = attendance30Days.Count(a => a.Status == "Present");
                StudentDashboard.TotalClassesMissedLast30Days = attendance30Days.Count(a => a.Status == "Absent");

                var total30Days = StudentDashboard.TotalClassesAttendedLast30Days + StudentDashboard.TotalClassesMissedLast30Days;
                StudentDashboard.AttendancePercentageLast30Days = total30Days > 0
                    ? (decimal)(StudentDashboard.TotalClassesAttendedLast30Days * 100.0 / total30Days)
                    : 0;


                var allAnnouncements = await _announcementService.GetAllAnnouncementsAsync();
                StudentDashboard.Announcements = allAnnouncements
                    .OrderByDescending(a => a.PublishedDate)
                    .Take(5)
                    .Select(a => new AnnouncementVM
                    {
                        AnnouncementId = a.Id,
                        Title = a.Title,
                        Content = a.Content,
                        PublishedDate = a.PublishedDate,
                        Author = "Teacher",
                        TeacherName = "Teacher"
                    })
                    .ToList();


                StudentDashboard.RecentGrades = gradesList
                    .OrderByDescending(g => g.GradedDate)
                    .Take(5)
                    .Select(g => new GradeVM
                    {
                        GradeId = g.Id,
                        CourseName = "Course",
                        Score = g.Score,
                        LetterGrade = g.LetterGrade,
                        GradedDate = g.GradedDate,
                        StudentName = "Student"
                    })
                    .ToList();


                var chartLabels = new[] { "Week 1", "Week 2", "Week 3", "Week 4" };
                var classworkScores = new[] { 8, 9, 7, 8 };
                var homeworkScores = new[] { 7, 8, 9, 8 };
                var controlScores = new[] { 6, 7, 8, 7 };
                var thematicScores = new[] { 8, 8, 9, 9 };

                StudentDashboard.AverageScoreChartData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    labels = chartLabels,
                    datasets = new[]
                    {
                        new { label = "Classwork", data = classworkScores, borderColor = "#5e5ce6", backgroundColor = "rgba(94,92,230,0.05)" },
                        new { label = "Homework", data = homeworkScores, borderColor = "#0066ff", backgroundColor = "rgba(0,102,255,0.05)" },
                        new { label = "Control", data = controlScores, borderColor = "#ff9500", backgroundColor = "rgba(255,149,0,0.05)" },
                        new { label = "Thematic", data = thematicScores, borderColor = "#d946ef", backgroundColor = "rgba(217,70,239,0.05)" }
                    }
                });
            }
            catch (Exception ex)
            {
                StudentDashboard = new StudentDashboardViewModel();
            }
        }


        private async Task LoadTeacherDashboard(string userId)
        {
            var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(userId);
            if (teacher == null)
                return;

            TeacherDashboard = new TeacherDashboardViewModel();


            var assignedGroups = await _groupService.GetGroupsByTeacherAsync(teacher.Id);
            TeacherDashboard.AssignedGroups = assignedGroups
                .Select(g => new GroupTeachingVM
                {
                    GroupId = g.Id,
                    GroupName = g.GroupName ?? "Group",
                    GroupCode = g.GroupCode ?? "N/A",
                    StudentCount = g.StudentCount,
                    CourseId = Guid.Empty,
                    CourseName = "Course",
                    CourseCode = "N/A"
                })
                .ToList();

            TeacherDashboard.TotalGroupsAssigned = TeacherDashboard.AssignedGroups.Count;
            TeacherDashboard.TotalStudentsManaging = TeacherDashboard.AssignedGroups.Sum(g => g.StudentCount);


            var todayLessons = await _lessonService.GetTodayLessonsForTeacherAsync(teacher.Id);
            TeacherDashboard.TodayLessons = todayLessons
                .Select(l => new LessonTeachingVM
                {
                    LessonId = l.Id,
                    Title = l.Title,
                    LessonDate = l.LessonDate,
                    DayOfWeek = l.LessonDate.ToString("dddd"),
                    Time = l.LessonDate.ToString("HH:mm"),
                    Location = l.Location ?? "TBD",
                    CourseName = l.CourseName ?? "Course"
                })
                .ToList();

            TeacherDashboard.LessonsToday = TeacherDashboard.TodayLessons.Count;


            var weekLessons = await _lessonService.GetLessonsByTeacherAsync(teacher.Id);
            var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var weekLessonsList = weekLessons
                .Where(l => l.LessonDate.Date >= startOfWeek && l.LessonDate.Date <= endOfWeek)
                .OrderBy(l => l.LessonDate)
                .ToList();


            if (!weekLessonsList.Any())
            {
                weekLessonsList = weekLessons
                    .OrderBy(l => l.LessonDate)
                    .Take(10)
                    .ToList();
            }

            TeacherDashboard.WeekLessons = weekLessonsList
                .Select(l => new LessonTeachingVM
                {
                    LessonId = l.Id,
                    Title = l.Title,
                    LessonDate = l.LessonDate,
                    DayOfWeek = l.LessonDate.ToString("ddd"),
                    Time = l.LessonDate.ToString("HH:mm"),
                    Location = l.Location ?? "TBD",
                    CourseName = l.CourseName ?? "Course"
                })
                .ToList();


            var pendingHomework = await _homeworkService.GetPendingHomeworkForTeacherAsync(teacher.Id);
            TeacherDashboard.PendingHomework = pendingHomework
                .OrderBy(h => h.DueDate)
                .Take(10)
                .Select(h => new HomeworkGradingVM
                {
                    HomeworkId = h.Id,
                    Title = h.Title,
                    DueDate = h.DueDate,
                    CourseName = h.CourseName ?? "Course",
                    PendingCount = 0
                })
                .ToList();

            TeacherDashboard.PendingHomeworkCount = TeacherDashboard.PendingHomework.Count;


            var teacherCourses = await _courseService.GetAllCoursesAsync();
            TeacherDashboard.ActiveCourses = teacherCourses.Count(c => c.IsActive && c.TeacherId == teacher.Id);

            var teacherLessons = await _lessonService.GetLessonsByTeacherAsync(teacher.Id);
            TeacherDashboard.TotalLessonsCreated = teacherLessons.Count();
        }


        private async Task LoadAdminDashboard()
        {
            AdminDashboard = new AdminDashboardViewModel();


            AdminDashboard.SystemOverview.TotalUsers = (await _studentService.GetAllStudentsAsync()).Count()
                + (await _teacherService.GetAllTeachersAsync()).Count();
            AdminDashboard.SystemOverview.TotalStudents = (await _studentService.GetAllStudentsAsync()).Count();
            AdminDashboard.SystemOverview.TotalTeachers = (await _teacherService.GetAllTeachersAsync()).Count();
            AdminDashboard.SystemOverview.TotalCourses = (await _courseService.GetAllCoursesAsync()).Count();
            AdminDashboard.SystemOverview.TotalGroups = (await _groupService.GetAllGroupsAsync()).Count();
            AdminDashboard.SystemOverview.TotalLessons = (await _lessonService.GetAllLessonsAsync()).Count();
            AdminDashboard.SystemOverview.TotalHomework = (await _homeworkService.GetAllHomeworkAsync()).Count();


            AdminDashboard.UserStatistics.ActiveStudents = AdminDashboard.SystemOverview.TotalStudents;
            AdminDashboard.UserStatistics.ActiveTeachers = AdminDashboard.SystemOverview.TotalTeachers;
            AdminDashboard.UserStatistics.NewUsersThisMonth = 0;


            var allCourses = await _courseService.GetAllCoursesAsync();
            AdminDashboard.AcademicStatistics.TotalActiveCourses = allCourses.Count(c => c.IsActive);
            AdminDashboard.AcademicStatistics.TotalInactiveCourses = allCourses.Count(c => !c.IsActive);


            AdminDashboard.RecentActivity = new List<ActivityItemVM>();


            var announcements = await _announcementService.GetRecentAnnouncementsAsync(5);
            AdminDashboard.RecentAnnouncements = announcements
                .Select(a => new AnnouncementVM
                {
                    AnnouncementId = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    PublishedDate = a.PublishedDate,
                    Author = a.TeacherName ?? "System"
                })
                .ToList();


            AdminDashboard.QuickActions = new List<UserManagementActionVM>
            {
                new() { Label = "Create Student", Icon = "fas fa-user-plus", Url = "/Students/Create", Color = "success" },
                new() { Label = "Create Teacher", Icon = "fas fa-chalkboard-user", Url = "/Teachers/Create", Color = "warning" },
                new() { Label = "Create Course", Icon = "fas fa-book", Url = "/Courses/Create", Color = "info" },
                new() { Label = "Manage Groups", Icon = "fas fa-users", Url = "/Groups", Color = "primary" },
            };
        }

        private async Task PrepareChartDataAsync()
        {

            var courses = (await _courseService.GetAllCoursesAsync()).ToList();
            var grades = (await _gradeService.GetAllGradesAsync()).ToList();
            var attendance = (await _attendanceService.GetAllAttendanceAsync()).ToList();
            var lessons = (await _lessonService.GetAllLessonsAsync()).ToList();


            var activeCourses = courses.Count(c => c.IsActive);
            var inactiveCourses = courses.Count(c => !c.IsActive);
            var coursesData = new
            {
                labels = new[] { "Active", "Inactive" },
                datasets = new[] {
                    new {
                        label = "Courses",
                        data = new[] { activeCourses, inactiveCourses },
                        backgroundColor = new[] { "#34c759", "#a1a1a6" }
                    }
                }
            };
            CoursesChartData = System.Text.Json.JsonSerializer.Serialize(coursesData);


            var enrollmentByMonth = lessons
                .GroupBy(l => l.CreatedAt.ToString("MMM"))
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            var enrollmentLabels = enrollmentByMonth.Any()
                ? enrollmentByMonth.Select(x => x.Month).ToArray()
                : new[] { "No Data" };
            var enrollmentValues = enrollmentByMonth.Any()
                ? enrollmentByMonth.Select(x => x.Count).ToArray()
                : new[] { 0 };

            var enrollmentData = new
            {
                labels = enrollmentLabels,
                datasets = new[] {
                    new {
                        label = "Enrollments",
                        data = enrollmentValues,
                        borderColor = "#5e5ce6",
                        backgroundColor = "rgba(94,92,230,0.1)",
                        fill = true
                    }
                }
            };
            EnrollmentChartData = System.Text.Json.JsonSerializer.Serialize(enrollmentData);


            var gradeDistribution = new Dictionary<string, int>
            {
                { "A", grades.Count(g => g.Score >= 90) },
                { "B", grades.Count(g => g.Score >= 80 && g.Score < 90) },
                { "C", grades.Count(g => g.Score >= 70 && g.Score < 80) },
                { "D", grades.Count(g => g.Score >= 60 && g.Score < 70) },
                { "F", grades.Count(g => g.Score < 60) }
            };

            var gradesData = new
            {
                labels = new[] { "A (90+)", "B (80-89)", "C (70-79)", "D (60-69)", "F (<60)" },
                datasets = new[] {
                    new {
                        label = "Students",
                        data = new[] {
                            gradeDistribution["A"],
                            gradeDistribution["B"],
                            gradeDistribution["C"],
                            gradeDistribution["D"],
                            gradeDistribution["F"]
                        },
                        backgroundColor = new[] { "#34c759", "#5e5ce6", "#0066ff", "#ff9500", "#ff3b30" }
                    }
                }
            };
            GradesChartData = System.Text.Json.JsonSerializer.Serialize(gradesData);


            var attendanceByWeek = new List<int>();
            var weekLabels = new List<string>();

            if (attendance.Any())
            {
                var weeks = attendance
                    .GroupBy(a => System.Globalization.CultureInfo.InvariantCulture
                        .Calendar.GetWeekOfYear(a.AttendanceDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .OrderBy(g => g.Key)
                    .Take(5)
                    .ToList();

                foreach (var week in weeks)
                {
                    var presentCount = week.Count(a => a.Status == "Present");
                    var totalCount = week.Count();
                    var percentage = totalCount > 0 ? (presentCount * 100 / totalCount) : 0;
                    attendanceByWeek.Add(percentage);
                    weekLabels.Add($"Week {week.Key}");
                }
            }

            var attendanceData = new
            {
                labels = weekLabels.Any() ? weekLabels.ToArray() : new[] { "No Data" },
                datasets = new[] {
                    new {
                        label = "Attendance %",
                        data = attendanceByWeek.Any() ? attendanceByWeek.ToArray() : new[] { 0 },
                        borderColor = "#34c759",
                        backgroundColor = "rgba(52,199,89,0.1)",
                        fill = true
                    }
                }
            };
            AttendanceChartData = System.Text.Json.JsonSerializer.Serialize(attendanceData);


            var activityByDay = new Dictionary<string, int>
            {
                { "Monday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Monday) },
                { "Tuesday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Tuesday) },
                { "Wednesday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Wednesday) },
                { "Thursday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Thursday) },
                { "Friday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Friday) },
                { "Saturday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Saturday) },
                { "Sunday", lessons.Count(l => l.CreatedAt.DayOfWeek == DayOfWeek.Sunday) }
            };

            var activityData = new
            {
                labels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                datasets = new[] {
                    new {
                        label = "User Activity",
                        data = new[] {
                            activityByDay["Monday"],
                            activityByDay["Tuesday"],
                            activityByDay["Wednesday"],
                            activityByDay["Thursday"],
                            activityByDay["Friday"],
                            activityByDay["Saturday"],
                            activityByDay["Sunday"]
                        },
                        borderColor = "#0066ff",
                        backgroundColor = "rgba(0,102,255,0.1)"
                    }
                }
            };
            ActivityChartData = System.Text.Json.JsonSerializer.Serialize(activityData);
        }

        private string GetTimeSince(DateTime date)
        {
            var now = DateTime.Now;
            var diff = now - date;

            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return date.ToString("MMM dd");
        }
    }
}