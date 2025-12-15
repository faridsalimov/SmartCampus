using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Lessons
{
    [Authorize(Roles = "Admin,Teacher")]
    public class EditModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public EditModel(
            ILessonService lessonService,
            IGroupService groupService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _lessonService = lessonService;
            _groupService = groupService;
            _teacherService = teacherService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        [BindProperty]
        public LessonDto Lesson { get; set; } = new();

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id);

            if (lesson == null)
            {
                return NotFound();
            }


            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher == null || lesson.TeacherId != teacher.Id)
                    return Unauthorized();
            }

            Lesson = lesson;


            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                RedirectToPage("/Account/Login");
                return Page();
            }

            try
            {

                if (User.IsInRole("Admin"))
                {
                    var allGroups = await _groupService.GetAllGroupsAsync();
                    Groups = allGroups.ToList();
                }
                else if (User.IsInRole("Teacher"))
                {
                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(currentUser.Id);
                    if (teacher != null)
                    {
                        var teacherGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(currentUser.Id);
                        if (teacherGroupIds.Any())
                        {
                            var allGroups = await _groupService.GetAllGroupsAsync();
                            Groups = allGroups.Where(g => teacherGroupIds.Contains(g.Id)).ToList();
                        }
                        else
                        {

                            var activeGroups = await _groupService.GetActiveGroupsAsync();
                            Groups = activeGroups.ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error loading groups: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Lesson.Id);
                return Page();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);


                if (userRoles.Contains("Teacher"))
                {
                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                    var existingLesson = await _lessonService.GetLessonByIdAsync(Lesson.Id);

                    if (teacher == null || existingLesson == null || existingLesson.TeacherId != teacher.Id)
                        return Unauthorized();
                }

                await _lessonService.UpdateLessonAsync(Lesson);
                ToastHelper.ShowSuccess(this, "Lesson updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating lesson: {ex.Message}");
                await OnGetAsync(Lesson.Id);
                return Page();
            }
        }
    }
}