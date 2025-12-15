using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Web.Pages.Teachers
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ITeacherService _teacherService;

        public DetailsModel(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        public TeacherDto? Teacher { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Teacher = await _teacherService.GetTeacherByIdAsync(id);

            if (Teacher == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}