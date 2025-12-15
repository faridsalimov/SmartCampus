using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IGradeService
    {
        Task<IEnumerable<GradeDto>> GetAllGradesAsync();
        Task<GradeDto?> GetGradeByIdAsync(Guid id);
        Task<IEnumerable<GradeDto>> GetGradesByStudentAsync(Guid studentId);
        Task<IEnumerable<GradeDto>> GetGradesByCourseAsync(Guid courseId);
        Task<IEnumerable<GradeDto>> GetGradesByGroupAsync(Guid groupId);
        Task<IEnumerable<GradeDto>> GetTeacherGradesAsync(Guid teacherId);
        Task<decimal?> GetStudentAverageGradeAsync(Guid studentId);
        Task<decimal?> GetGroupAverageGradeAsync(Guid groupId);
        Task<GradeDto> GradeHomeworkSubmissionAsync(Guid submissionId, decimal score, string? feedback);
        Task<GradeDto> CreateGradeAsync(GradeDto gradeDto);
        Task UpdateGradeAsync(GradeDto gradeDto);
        Task DeleteGradeAsync(Guid id);
    }
}