namespace SmartCampus.Web.ViewModels
{

    public class StudentDashboardViewModel
    {

        public GroupOverviewVM GroupOverview { get; set; } = new();


        public decimal CurrentGPA { get; set; }
        public decimal AttendancePercentage { get; set; }
        public int EnrolledCoursesCount { get; set; }


        public int TotalClassesAttendedLast30Days { get; set; }
        public int TotalClassesMissedLast30Days { get; set; }
        public decimal AttendancePercentageLast30Days { get; set; }
        public List<AttendanceStatVM> AttendanceStats { get; set; } = new();


        public List<LessonOverviewVM> UpcomingLessons { get; set; } = new();


        public List<HomeworkSummaryVM> Homeworks { get; set; } = new();
        public int PendingHomeworkCount { get; set; }
        public int CompletedHomeworkCount { get; set; }
        public int OverdueHomeworkCount { get; set; }


        public int TotalClassesAttended { get; set; }
        public int TotalClassesMissed { get; set; }
        public List<AttendanceRecordVM> RecentAttendance { get; set; } = new();


        public List<AnnouncementVM> Announcements { get; set; } = new();


        public List<GradeVM> RecentGrades { get; set; } = new();


        public string AverageScoreChartData { get; set; } = "{}";


        public List<LeaderboardEntryVM> Leaderboard { get; set; } = new();
        public int StudentRank { get; set; }
        public int StudentScore { get; set; }
    }

    public class AttendanceStatVM
    {
        public string Month { get; set; } = string.Empty;
        public decimal VisitingPercentage { get; set; }
        public decimal PassPercentage { get; set; }
        public decimal LatenessPercentage { get; set; }
    }

    public class GroupOverviewVM
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public List<StudentSummaryVM> Groupmates { get; set; } = new();
    }

    public class StudentSummaryVM
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class LessonOverviewVM
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }

    public class HomeworkSummaryVM
    {
        public Guid HomeworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Grade { get; set; }
        public int DaysUntilDue { get; set; }
        public string Type { get; set; } = "Task";
    }

    public class AttendanceRecordVM
    {
        public Guid AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
    }

    public class AnnouncementVM
    {
        public Guid AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string? Author { get; set; }
        public bool IsGroupSpecific { get; set; }
        public string? TeacherName { get; set; }
    }

    public class GradeVM
    {
        public Guid GradeId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public DateTime GradedDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
    }

    public class LeaderboardEntryVM
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal Score { get; set; }
        public string? Avatar { get; set; }
    }
}