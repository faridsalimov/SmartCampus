using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IGroupService
    {
        Task<IEnumerable<GroupDto>> GetAllGroupsAsync();
        Task<GroupDto?> GetGroupByIdAsync(Guid id);
        Task<GroupDto?> GetGroupByCodeAsync(string code);
        Task<IEnumerable<GroupDto>> GetActiveGroupsAsync();


        Task<IEnumerable<GroupDto>> GetGroupsByTeacherAsync(Guid teacherId);
        Task<GroupDto?> GetStudentGroupAsync(Guid studentId);

        Task<GroupDto> CreateGroupAsync(GroupDto groupDto);
        Task UpdateGroupAsync(GroupDto groupDto);
        Task DeleteGroupAsync(Guid id);
    }
}