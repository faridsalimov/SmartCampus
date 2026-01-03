using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TeacherService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TeacherDto>> GetAllTeachersAsync()
        {
            var teachers = await _unitOfWork.TeacherRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TeacherDto>>(teachers);
        }

        public async Task<TeacherDto?> GetTeacherByIdAsync(Guid id)
        {
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(id);
            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task<TeacherDto?> GetTeacherByApplicationUserIdAsync(string applicationUserId)
        {
            var teacher = await _unitOfWork.TeacherRepository.GetByApplicationUserIdAsync(applicationUserId);
            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task<IEnumerable<TeacherDto>> GetTeachersByDepartmentAsync(string department)
        {
            var teachers = await _unitOfWork.TeacherRepository.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<TeacherDto>>(teachers);
        }


        public async Task<int> GetTotalStudentsForTeacherAsync(Guid teacherId)
        {

            var allStudents = await _unitOfWork.StudentRepository.GetAllAsync();
            return allStudents.Count();
        }

        public async Task<TeacherDto> CreateTeacherAsync(TeacherDto teacherDto)
        {
            if (teacherDto == null)
                throw new ArgumentNullException(nameof(teacherDto));

            if (string.IsNullOrWhiteSpace(teacherDto.ApplicationUserId))
                throw new InvalidOperationException("ApplicationUserId is required.");

            try
            {
                var existingAppUser = await _unitOfWork.TeacherRepository.GetByApplicationUserIdAsync(teacherDto.ApplicationUserId);
                if (existingAppUser != null)
                    throw new InvalidOperationException("This user is already linked to another teacher record.");

                var teacher = _mapper.Map<Teacher>(teacherDto);
                teacher.Id = Guid.NewGuid();
                teacher.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.TeacherRepository.AddAsync(teacher);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<TeacherDto>(teacher);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create teacher: {ex.Message}", ex);
            }
        }

        public async Task UpdateTeacherAsync(TeacherDto teacherDto)
        {
            if (teacherDto == null)
                throw new ArgumentNullException(nameof(teacherDto));

            if (teacherDto.Id == Guid.Empty)
                throw new InvalidOperationException("Teacher Id is required for update.");

            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherDto.Id);
                if (teacher == null)
                    throw new InvalidOperationException($"Teacher with Id {teacherDto.Id} not found.");

                teacher = _mapper.Map(teacherDto, teacher);
                teacher.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.TeacherRepository.Update(teacher);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update teacher: {ex.Message}", ex);
            }
        }

        public async Task DeleteTeacherAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Teacher Id is required.");

            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(id);
                if (teacher == null)
                    throw new InvalidOperationException($"Teacher with Id {id} not found.");

                _unitOfWork.TeacherRepository.Delete(teacher);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete teacher: {ex.Message}", ex);
            }
        }
    }
}