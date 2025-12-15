using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByTeacherAsync(Guid teacherId)
        {
            var courses = await _unitOfWork.CourseRepository.GetByTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByGroupAsync(Guid groupId)
        {
            var courses = await _unitOfWork.CourseRepository.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetCourseBycodeAsync(string code)
        {
            var course = await _unitOfWork.CourseRepository.GetByCourseCodeAsync(code);
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CourseDto courseDto)
        {
            if (courseDto == null)
                throw new ArgumentNullException(nameof(courseDto));

            if (courseDto.TeacherId == Guid.Empty)
                throw new InvalidOperationException("Teacher ID is required.");

            try
            {
                var course = _mapper.Map<Course>(courseDto);
                course.Id = Guid.NewGuid();
                course.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.CourseRepository.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<CourseDto>(course);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create course: {ex.Message}", ex);
            }
        }

        public async Task UpdateCourseAsync(CourseDto courseDto)
        {
            if (courseDto == null)
                throw new ArgumentNullException(nameof(courseDto));

            if (courseDto.Id == Guid.Empty)
                throw new InvalidOperationException("Course ID is required for update.");

            try
            {
                var course = _mapper.Map<Course>(courseDto);
                course.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.CourseRepository.Update(course);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update course: {ex.Message}", ex);
            }
        }

        public async Task DeleteCourseAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Course ID is required.");

            try
            {
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
                if (course == null)
                    throw new InvalidOperationException($"Course with ID {id} not found.");

                _unitOfWork.CourseRepository.Delete(course);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete course: {ex.Message}", ex);
            }
        }
    }
}