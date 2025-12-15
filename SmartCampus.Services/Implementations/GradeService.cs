using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GradeDto>> GetAllGradesAsync()
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task<GradeDto?> GetGradeByIdAsync(Guid id)
        {
            var grade = await _unitOfWork.GradeRepository.GetByIdAsync(id);
            return _mapper.Map<GradeDto>(grade);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByStudentAsync(Guid studentId)
        {
            var grades = await _unitOfWork.GradeRepository.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByCourseAsync(Guid courseId)
        {
            var grades = await _unitOfWork.GradeRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByGroupAsync(Guid groupId)
        {
            var grades = await _unitOfWork.GradeRepository.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task<IEnumerable<GradeDto>> GetTeacherGradesAsync(Guid teacherId)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                    throw new InvalidOperationException("Teacher not found.");


                var lessons = await _unitOfWork.LessonRepository.GetByTeacherIdAsync(teacherId);


                var teachingGroupIds = lessons
                    .Select(l => l.GroupId)
                    .Distinct()
                    .ToList();


                var allGrades = await _unitOfWork.GradeRepository.GetAllAsync();


                var filteredGrades = allGrades
                    .Where(g => g.GroupId.HasValue && teachingGroupIds.Contains(g.GroupId.Value))
                    .ToList();

                return _mapper.Map<IEnumerable<GradeDto>>(filteredGrades);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving teacher grades: {ex.Message}", ex);
            }
        }

        public async Task<decimal?> GetStudentAverageGradeAsync(Guid studentId)
        {
            return await _unitOfWork.GradeRepository.GetStudentAverageGradeAsync(studentId);
        }

        public async Task<decimal?> GetGroupAverageGradeAsync(Guid groupId)
        {
            return await _unitOfWork.GradeRepository.GetGroupAverageGradeAsync(groupId);
        }

        public async Task<GradeDto> GradeHomeworkSubmissionAsync(Guid submissionId, decimal score, string? feedback)
        {
            var submission = await _unitOfWork.HomeworkSubmissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
                throw new InvalidOperationException("Homework submission not found.");

            var grade = new Grade
            {
                Id = Guid.NewGuid(),
                StudentId = submission.StudentId,
                HomeworkSubmissionId = submissionId,
                Score = score,
                LetterGrade = GetLetterGrade(score),
                Feedback = feedback,
                GradeType = "Homework",
                GradedDate = DateTime.UtcNow
            };

            await _unitOfWork.GradeRepository.AddAsync(grade);
            submission.Status = "Graded";
            _unitOfWork.HomeworkSubmissionRepository.Update(submission);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GradeDto>(grade);
        }

        public async Task<GradeDto> CreateGradeAsync(GradeDto gradeDto)
        {
            if (gradeDto == null)
                throw new ArgumentNullException(nameof(gradeDto));

            if (gradeDto.StudentId == Guid.Empty)
                throw new InvalidOperationException("Student ID is required.");

            if (gradeDto.Score < 0 || gradeDto.Score > 100)
                throw new InvalidOperationException("Score must be between 0 and 100.");

            try
            {

                if (gradeDto.CourseId.HasValue && gradeDto.CourseId != Guid.Empty)
                {
                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(gradeDto.CourseId.Value);
                    if (course == null)
                        throw new InvalidOperationException($"Course with ID {gradeDto.CourseId} not found.");
                }

                var grade = _mapper.Map<Grade>(gradeDto);
                grade.Id = Guid.NewGuid();
                grade.LetterGrade = GetLetterGrade(grade.Score);
                grade.GradedDate = DateTime.UtcNow;

                await _unitOfWork.GradeRepository.AddAsync(grade);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<GradeDto>(grade);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create grade: {ex.Message}", ex);
            }
        }

        public async Task UpdateGradeAsync(GradeDto gradeDto)
        {
            if (gradeDto == null)
                throw new ArgumentNullException(nameof(gradeDto));

            if (gradeDto.Id == Guid.Empty)
                throw new InvalidOperationException("Grade ID is required for update.");

            try
            {

                if (gradeDto.CourseId.HasValue && gradeDto.CourseId != Guid.Empty)
                {
                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(gradeDto.CourseId.Value);
                    if (course == null)
                        throw new InvalidOperationException($"Course with ID {gradeDto.CourseId} not found.");
                }

                var grade = _mapper.Map<Grade>(gradeDto);
                grade.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.GradeRepository.Update(grade);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update grade: {ex.Message}", ex);
            }
        }

        public async Task DeleteGradeAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Grade ID is required.");

            try
            {
                var grade = await _unitOfWork.GradeRepository.GetByIdAsync(id);
                if (grade == null)
                    throw new InvalidOperationException($"Grade with ID {id} not found.");

                _unitOfWork.GradeRepository.Delete(grade);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete grade: {ex.Message}", ex);
            }
        }

        private string GetLetterGrade(decimal score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}