using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync(string? searchTerm = null)
        {
            SearchTerm = searchTerm;
            var courses = await _courseService.GetAllCoursesAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                courses = courses.Where(c =>
                    (c.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Code?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Courses = courses.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _courseService.DeleteCourseAsync(id);
                ToastHelper.ShowSuccess(this, "Course deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting course: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}