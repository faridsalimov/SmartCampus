using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Web.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;

        public DetailsModel(ICourseService courseService, ILessonService lessonService)
        {
            _courseService = courseService;
            _lessonService = lessonService;
        }

        public CourseDto? Course { get; set; }
        public IList<LessonDto> Lessons { get; set; } = new List<LessonDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Course = await _courseService.GetCourseByIdAsync(id);

            if (Course == null)
            {
                return NotFound();
            }

            Lessons = (await _lessonService.GetLessonsByCourseAsync(id)).ToList();

            return Page();
        }
    }
}