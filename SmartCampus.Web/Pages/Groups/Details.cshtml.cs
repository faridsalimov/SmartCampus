using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Web.Pages.Groups
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;

        public DetailsModel(IGroupService groupService, IStudentService studentService)
        {
            _groupService = groupService;
            _studentService = studentService;
        }

        public GroupDto? Group { get; set; }
        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Group = await _groupService.GetGroupByIdAsync(id);

            if (Group == null)
            {
                return NotFound();
            }

            Students = (await _studentService.GetStudentsByGroupAsync(id)).ToList();

            return Page();
        }
    }
}