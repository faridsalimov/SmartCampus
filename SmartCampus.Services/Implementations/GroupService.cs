using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GroupService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await _unitOfWork.GroupRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GroupDto>>(groups);
        }

        public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
        {
            var group = await _unitOfWork.GroupRepository.GetByIdAsync(id);
            return _mapper.Map<GroupDto>(group);
        }

        public async Task<GroupDto?> GetGroupByCodeAsync(string code)
        {
            var group = await _unitOfWork.GroupRepository.GetByCodeAsync(code);
            return _mapper.Map<GroupDto>(group);
        }

        public async Task<IEnumerable<GroupDto>> GetActiveGroupsAsync()
        {
            var groups = await _unitOfWork.GroupRepository.GetActiveGroupsAsync();
            var groupDtos = _mapper.Map<IEnumerable<GroupDto>>(groups).ToList();

            foreach (var groupDto in groupDtos)
            {
                groupDto.StudentCount = await _unitOfWork.GroupRepository.GetStudentCountByGroupAsync(groupDto.Id);
            }

            return groupDtos;
        }


        public async Task<IEnumerable<GroupDto>> GetGroupsByTeacherAsync(Guid teacherId)
        {

            var allGroups = await GetAllGroupsAsync();

            return await GetActiveGroupsAsync();
        }


        public async Task<GroupDto?> GetStudentGroupAsync(Guid studentId)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
            if (student == null || student.GroupId == Guid.Empty)
                return null;

            return await GetGroupByIdAsync(student.GroupId);
        }

        public async Task<GroupDto> CreateGroupAsync(GroupDto groupDto)
        {
            var group = _mapper.Map<Group>(groupDto);
            group.Id = Guid.NewGuid();
            group.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.GroupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GroupDto>(group);
        }

        public async Task UpdateGroupAsync(GroupDto groupDto)
        {
            var group = _mapper.Map<Group>(groupDto);
            group.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GroupRepository.Update(group);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteGroupAsync(Guid id)
        {
            var group = await _unitOfWork.GroupRepository.GetByIdAsync(id);
            if (group != null)
            {
                _unitOfWork.GroupRepository.Delete(group);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}