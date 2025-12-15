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
            var teacher = _mapper.Map<Teacher>(teacherDto);
            teacher.Id = Guid.NewGuid();
            teacher.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.TeacherRepository.AddAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task UpdateTeacherAsync(TeacherDto teacherDto)
        {
            var teacher = _mapper.Map<Teacher>(teacherDto);
            teacher.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TeacherRepository.Update(teacher);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteTeacherAsync(Guid id)
        {
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(id);
            if (teacher != null)
            {
                _unitOfWork.TeacherRepository.Delete(teacher);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}