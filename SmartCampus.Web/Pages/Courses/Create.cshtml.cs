using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;

        public CreateModel(
            ICourseService courseService,
            ITeacherService teacherService,
            IGroupService groupService)
        {
            _courseService = courseService;
            _teacherService = teacherService;
            _groupService = groupService;
        }

        [BindProperty]
        public CourseDto Course { get; set; } = new();

        public IList<TeacherDto> Teachers { get; set; } = new List<TeacherDto>();
        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public async Task OnGetAsync()
        {
            Teachers = (await _teacherService.GetAllTeachersAsync()).ToList();
            Groups = (await _groupService.GetAllGroupsAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Teachers = (await _teacherService.GetAllTeachersAsync()).ToList();
                Groups = (await _groupService.GetAllGroupsAsync()).ToList();
                return Page();
            }

            try
            {
                await _courseService.CreateCourseAsync(Course);
                ToastHelper.ShowSuccess(this, "Course created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating course: {ex.Message}");
                Teachers = (await _teacherService.GetAllTeachersAsync()).ToList();
                Groups = (await _groupService.GetAllGroupsAsync()).ToList();
                return Page();
            }
        }
    }
}