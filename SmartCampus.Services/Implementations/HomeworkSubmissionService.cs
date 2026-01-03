using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class HomeworkSubmissionService : IHomeworkSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeworkSubmissionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HomeworkSubmissionDto>> GetSubmissionsByHomeworkAsync(Guid homeworkId)
        {
            var submissions = await _unitOfWork.HomeworkSubmissionRepository.GetSubmissionsByHomeworkAsync(homeworkId);
            return _mapper.Map<IEnumerable<HomeworkSubmissionDto>>(submissions);
        }

        public async Task<IEnumerable<HomeworkSubmissionDto>> GetSubmissionsByStudentAsync(Guid studentId)
        {
            var submissions = await _unitOfWork.HomeworkSubmissionRepository.GetSubmissionsByStudentAsync(studentId);
            return _mapper.Map<IEnumerable<HomeworkSubmissionDto>>(submissions);
        }

        public async Task<HomeworkSubmissionDto?> GetSubmissionByIdAsync(Guid id)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetByIdAsync(id);
            return _mapper.Map<HomeworkSubmissionDto?>(submission);
        }

        public async Task<HomeworkSubmissionDto?> GetStudentSubmissionAsync(Guid homeworkId, Guid studentId)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetStudentSubmissionAsync(homeworkId, studentId);
            return _mapper.Map<HomeworkSubmissionDto?>(submission);
        }

        public async Task<HomeworkSubmissionDto> CreateSubmissionAsync(HomeworkSubmissionDto submissionDto)
        {
            var submission = _mapper.Map<HomeworkSubmission>(submissionDto);
            submission.Id = Guid.NewGuid();
            submission.SubmissionDate = DateTime.UtcNow;
            submission.Status = "Submitted";

            await _unitOfWork.HomeworkSubmissionRepository.AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<HomeworkSubmissionDto>(submission);
        }

        public async Task<HomeworkSubmissionDto> GradeSubmissionAsync(Guid submissionId, int score, string? comments)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
                throw new KeyNotFoundException($"Submission with ID {submissionId} not found.");

            submission.Status = "Graded";
            submission.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.HomeworkSubmissionRepository.Update(submission);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<HomeworkSubmissionDto>(submission);
        }

        public async Task UpdateSubmissionStatusAsync(Guid submissionId, string status)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
                throw new KeyNotFoundException($"Submission with ID {submissionId} not found.");

            submission.Status = status;
            submission.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.HomeworkSubmissionRepository.Update(submission);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteSubmissionAsync(Guid id)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetByIdAsync(id);
            if (submission != null)
            {
                _unitOfWork.HomeworkSubmissionRepository.Delete(submission);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<HomeworkSubmissionDto>> GetUngradedSubmissionsAsync(Guid homeworkId)
        {
            var submissions = await _unitOfWork.HomeworkSubmissionRepository.GetUngradedSubmissionsAsync(homeworkId);
            return _mapper.Map<IEnumerable<HomeworkSubmissionDto>>(submissions);
        }

        public async Task<bool> HasStudentSubmittedAsync(Guid homeworkId, Guid studentId)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetStudentSubmissionAsync(homeworkId, studentId);
            return submission != null;
        }

        private string GetLetterGrade(int score)
        {
            return score switch
            {
                >= 9 => "A",
                >= 8 => "B",
                >= 7 => "C",
                >= 6 => "D",
                _ => "F"
            };
        }
    }
}