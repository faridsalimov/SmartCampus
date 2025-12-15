namespace SmartCampus.Web.Authorization
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        public static readonly string[] AllRoles = { Admin, Teacher, Student };
    }

    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string TeacherOnly = "TeacherOnly";
        public const string StudentOnly = "StudentOnly";
        public const string TeacherOrAdmin = "TeacherOrAdmin";
        public const string NotStudent = "NotStudent";
    }
}