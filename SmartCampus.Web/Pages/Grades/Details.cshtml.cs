using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Grades
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IGradeService _gradeService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;

        public DetailsModel(
            IGradeService gradeService,
            IStudentService studentService,
            ICourseService courseService)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _courseService = courseService;
        }

        public GradeDto? Grade { get; set; }
        public StudentDto? Student { get; set; }
        public CourseDto? Course { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Grade = await _gradeService.GetGradeByIdAsync(id);

            if (Grade == null)
            {
                return NotFound();
            }


            if (Grade.StudentId != Guid.Empty)
            {
                Student = await _studentService.GetStudentByIdAsync(Grade.StudentId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _gradeService.DeleteGradeAsync(id);
                ToastHelper.ShowSuccess(this, "Grade deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting grade: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}