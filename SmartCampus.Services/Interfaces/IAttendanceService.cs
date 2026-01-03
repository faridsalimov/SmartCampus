using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceDto>> GetAllAttendanceAsync();
        Task<AttendanceDto?> GetAttendanceByIdAsync(Guid id);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentAsync(Guid studentId);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByLessonAsync(Guid lessonId);
        Task<decimal> GetStudentAttendancePercentageAsync(Guid studentId);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByGroupAsync(Guid groupId);
        Task<int> GetPresentCountForStudentAsync(Guid studentId);
        Task<int> GetAbsentCountForStudentAsync(Guid studentId);
        Task<AttendanceDto> RecordAttendanceAsync(Guid studentId, Guid lessonId, string status);
        Task<IEnumerable<AttendanceDto>> CreateBulkAttendanceForLessonAsync(Guid lessonId);
        Task UpdateAttendanceAsync(AttendanceDto attendanceDto);
        Task DeleteAttendanceAsync(Guid id);
        Task<LessonSessionDto> StartLessonSessionAsync(Guid lessonId, Guid teacherId);
        Task EndLessonSessionAsync(Guid lessonId, Guid teacherId);
        Task<LessonSessionDto> GetActiveLessonSessionAsync(Guid lessonId, Guid teacherId);
        Task UpdateSessionAttendanceAsync(Guid lessonId, Guid studentId, string status, Guid teacherId);
        Task<IEnumerable<AttendanceDto>> GetSessionAttendanceAsync(Guid lessonId);
    }
}