using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class HomeworkService : IHomeworkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeworkService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HomeworkDto>> GetAllHomeworkAsync()
        {
            var homework = await _unitOfWork.HomeworkRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<HomeworkDto?> GetHomeworkByIdAsync(Guid id)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByIdAsync(id);
            return _mapper.Map<HomeworkDto>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetHomeworkByCourseAsync(Guid courseId)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetHomeworkByTeacherAsync(Guid teacherId)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetUpcomingHomeworkAsync(int days = 7)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetUpcomingHomeworkAsync(days);
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetHomeworkByGroupAsync(Guid groupId)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetHomeworkByStudentAsync(Guid studentId)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return Enumerable.Empty<HomeworkDto>();
            }

            var homework = await _unitOfWork.HomeworkRepository.GetByGroupIdAsync(student.GroupId);
            return _mapper.Map<IEnumerable<HomeworkDto>>(homework);
        }

        public async Task<IEnumerable<HomeworkDto>> GetPendingHomeworkForTeacherAsync(Guid teacherId)
        {
            var homework = await GetHomeworkByTeacherAsync(teacherId);
            return homework.Where(h => h.DueDate > DateTime.UtcNow).OrderBy(h => h.DueDate);
        }

        public async Task<HomeworkDto?> GetHomeworkByTeacherWithOwnershipCheckAsync(Guid homeworkId, Guid teacherId)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByIdAsync(homeworkId);
            if (homework == null)
            {
                return null;
            }


            if (homework.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this homework.");
            }

            return _mapper.Map<HomeworkDto>(homework);
        }

        public async Task MarkHomeworkCompletedByStudentAsync(Guid homeworkId, Guid studentId)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByIdAsync(homeworkId);
            if (homework == null)
            {
                throw new InvalidOperationException($"Homework with ID {homeworkId} not found.");
            }

            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new InvalidOperationException($"Student with ID {studentId} not found.");
            }


            if (student.GroupId != homework.GroupId)
            {
                throw new UnauthorizedAccessException("Student does not belong to the group this homework is assigned to.");
            }

            homework.IsCompleted = true;
            homework.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.HomeworkRepository.Update(homework);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<HomeworkDto> CreateHomeworkAsync(HomeworkDto homeworkDto)
        {
            var homework = _mapper.Map<Homework>(homeworkDto);
            homework.Id = Guid.NewGuid();
            homework.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.HomeworkRepository.AddAsync(homework);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<HomeworkDto>(homework);
        }

        public async Task UpdateHomeworkAsync(HomeworkDto homeworkDto)
        {
            var homework = _mapper.Map<Homework>(homeworkDto);
            homework.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.HomeworkRepository.Update(homework);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteHomeworkAsync(Guid id)
        {
            var homework = await _unitOfWork.HomeworkRepository.GetByIdAsync(id);
            if (homework != null)
            {
                _unitOfWork.HomeworkRepository.Delete(homework);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}