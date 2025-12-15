using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Teachers
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ITeacherService _teacherService;

        public EditModel(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [BindProperty]
        public TeacherDto Teacher { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            Teacher = teacher;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _teacherService.UpdateTeacherAsync(Teacher);
                ToastHelper.ShowSuccess(this, "Teacher updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating teacher: {ex.Message}");
                return Page();
            }
        }
    }
}