using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IHomeworkSubmissionRepository : IRepository<HomeworkSubmission>
    {
        Task<IEnumerable<HomeworkSubmission>> GetByHomeworkIdAsync(Guid homeworkId);
        Task<IEnumerable<HomeworkSubmission>> GetByStudentIdAsync(Guid studentId);
        Task<HomeworkSubmission?> GetByStudentAndHomeworkAsync(Guid studentId, Guid homeworkId);
        Task<IEnumerable<HomeworkSubmission>> GetUngradeSubmissionsAsync();




        Task<IEnumerable<HomeworkSubmission>> GetSubmissionsByHomeworkAsync(Guid homeworkId);




        Task<IEnumerable<HomeworkSubmission>> GetSubmissionsByStudentAsync(Guid studentId);




        Task<HomeworkSubmission?> GetStudentSubmissionAsync(Guid homeworkId, Guid studentId);




        Task<IEnumerable<HomeworkSubmission>> GetUngradedSubmissionsAsync(Guid homeworkId);
    }
}