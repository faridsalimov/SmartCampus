namespace SmartCampus.Web.ViewModels
{




    public class TeacherDashboardViewModel
    {

        public List<GroupTeachingVM> AssignedGroups { get; set; } = new();
        public int TotalGroupsAssigned { get; set; }
        public int TotalStudentsManaging { get; set; }


        public List<LessonTeachingVM> TodayLessons { get; set; } = new();
        public int LessonsToday { get; set; }


        public List<LessonTeachingVM> WeekLessons { get; set; } = new();


        public List<HomeworkGradingVM> PendingHomework { get; set; } = new();
        public int PendingHomeworkCount { get; set; }


        public List<LessonCreatedVM> RecentLessonsCreated { get; set; } = new();
        public List<AnnouncementVM> RecentAnnouncementsMade { get; set; } = new();


        public int ActiveCourses { get; set; }
        public int TotalLessonsCreated { get; set; }
        public int TotalAnnouncementsMade { get; set; }


        public List<GroupStudentStatsVM> GroupStudentStats { get; set; } = new();
    }

    public class GroupStudentStatsVM
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal AverageGPA { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class GroupTeachingVM
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
    }

    public class LessonTeachingVM
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int EnrolledStudents { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class HomeworkGradingVM
    {
        public Guid HomeworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SubmissionsCount { get; set; }
        public int GradedCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class LessonCreatedVM
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string GroupName { get; set; } = string.Empty;
    }
}