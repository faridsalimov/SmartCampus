using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync()
        {
            var lessons = await _unitOfWork.LessonRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(Guid id)
        {
            var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(id);
            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByCourseAsync(Guid courseId)
        {
            var lessons = await _unitOfWork.LessonRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByTeacherAsync(Guid teacherId)
        {
            var lessons = await _unitOfWork.LessonRepository.GetByTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<IEnumerable<LessonDto>> GetUpcomingLessonsAsync(int days = 7)
        {
            var lessons = await _unitOfWork.LessonRepository.GetUpcomingLessonsAsync(days);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByGroupAsync(Guid groupId)
        {
            var lessons = await _unitOfWork.LessonRepository.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<IEnumerable<LessonDto>> GetTodayLessonsForTeacherAsync(Guid teacherId)
        {
            var lessons = await GetLessonsByTeacherAsync(teacherId);
            var today = DateTime.Now.Date;

            return lessons
                .Where(l => l.LessonDate.Date == today)
                .OrderBy(l => l.LessonDate)
                .ToList();
        }

        public async Task<LessonDto?> GetLessonByTeacherWithOwnershipCheckAsync(Guid lessonId, Guid teacherId)
        {
            var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return null;
            }


            if (lesson.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this lesson.");
            }

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<LessonDto> CreateLessonAsync(LessonDto lessonDto)
        {
            var lesson = _mapper.Map<Lesson>(lessonDto);
            lesson.Id = Guid.NewGuid();
            lesson.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.LessonRepository.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task UpdateLessonAsync(LessonDto lessonDto)
        {
            var lesson = _mapper.Map<Lesson>(lessonDto);
            lesson.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.LessonRepository.Update(lesson);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteLessonAsync(Guid id)
        {
            var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(id);
            if (lesson != null)
            {
                _unitOfWork.LessonRepository.Delete(lesson);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task CompleteLessonAsync(Guid id)
        {
            var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(id);
            if (lesson != null)
            {
                lesson.IsCompleted = true;
                lesson.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.LessonRepository.Update(lesson);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}