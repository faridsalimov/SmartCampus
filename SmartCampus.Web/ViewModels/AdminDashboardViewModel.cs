namespace SmartCampus.Web.ViewModels
{




    public class AdminDashboardViewModel
    {

        public SystemOverviewVM SystemOverview { get; set; } = new();


        public UserStatisticsVM UserStatistics { get; set; } = new();


        public AcademicStatisticsVM AcademicStatistics { get; set; } = new();


        public List<ActivityItemVM> RecentActivity { get; set; } = new();
        public List<AnnouncementVM> RecentAnnouncements { get; set; } = new();
        public List<LessonCreatedVM> RecentLessonsCreated { get; set; } = new();


        public List<UserManagementActionVM> QuickActions { get; set; } = new();


        public SystemHealthVM SystemHealth { get; set; } = new();
    }

    public class SystemOverviewVM
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCourses { get; set; }
        public int TotalGroups { get; set; }
        public int TotalLessons { get; set; }
        public int TotalHomework { get; set; }
    }

    public class UserStatisticsVM
    {
        public int ActiveStudents { get; set; }
        public int ActiveTeachers { get; set; }
        public int ActiveAdmins { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double AverageUsersPerGroup { get; set; }
    }

    public class AcademicStatisticsVM
    {
        public int TotalActiveCourses { get; set; }
        public int TotalInactiveCourses { get; set; }
        public int AverageCourseSize { get; set; }
        public int TotalLessonsThisMonth { get; set; }
        public int TotalHomeworkAssigned { get; set; }
        public decimal AverageAttendanceRate { get; set; }
        public double AverageGPA { get; set; }
    }

    public class ActivityItemVM
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Actor { get; set; } = string.Empty;
    }

    public class UserManagementActionVM
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
    }

    public class SystemHealthVM
    {
        public bool IsHealthy { get; set; }
        public string HealthStatus { get; set; } = "Healthy";
        public int DatabaseSize { get; set; }
        public decimal DiskUsagePercentage { get; set; }
        public int LastBackupDaysAgo { get; set; }
        public List<string> RecentErrors { get; set; } = new();
    }
}