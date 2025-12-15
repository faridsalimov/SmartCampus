using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Students
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;

        public EditModel(IStudentService studentService, IGroupService groupService)
        {
            _studentService = studentService;
            _groupService = groupService;
        }

        [BindProperty]
        public StudentDto Student { get; set; } = new();

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            Groups = (await _groupService.GetAllGroupsAsync()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Groups = (await _groupService.GetAllGroupsAsync()).ToList();
                return Page();
            }

            try
            {
                await _studentService.UpdateStudentAsync(Student);
                ToastHelper.ShowSuccess(this, "Student updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating student: {ex.Message}");
                Groups = (await _groupService.GetAllGroupsAsync()).ToList();
                return Page();
            }
        }
    }
}