using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IGradeRepository : IRepository<Grade>
    {
        Task<IEnumerable<Grade>> GetByStudentIdAsync(Guid studentId);
        Task<IEnumerable<Grade>> GetByCourseIdAsync(Guid courseId);
        Task<IEnumerable<Grade>> GetByGroupIdAsync(Guid groupId);
        Task<Grade?> GetByHomeworkSubmissionIdAsync(Guid submissionId);
        Task<decimal?> GetStudentAverageGradeAsync(Guid studentId);
        Task<decimal?> GetGroupAverageGradeAsync(Guid groupId);
    }
}