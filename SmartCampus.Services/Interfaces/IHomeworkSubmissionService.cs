using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{



    public interface IHomeworkSubmissionService
    {



        Task<IEnumerable<HomeworkSubmissionDto>> GetSubmissionsByHomeworkAsync(Guid homeworkId);




        Task<IEnumerable<HomeworkSubmissionDto>> GetSubmissionsByStudentAsync(Guid studentId);




        Task<HomeworkSubmissionDto?> GetSubmissionByIdAsync(Guid id);




        Task<HomeworkSubmissionDto?> GetStudentSubmissionAsync(Guid homeworkId, Guid studentId);




        Task<HomeworkSubmissionDto> CreateSubmissionAsync(HomeworkSubmissionDto submissionDto);




        Task<HomeworkSubmissionDto> GradeSubmissionAsync(Guid submissionId, int score, string? comments);




        Task UpdateSubmissionStatusAsync(Guid submissionId, string status);




        Task DeleteSubmissionAsync(Guid id);




        Task<IEnumerable<HomeworkSubmissionDto>> GetUngradedSubmissionsAsync(Guid homeworkId);




        Task<bool> HasStudentSubmittedAsync(Guid homeworkId, Guid studentId);
    }
}