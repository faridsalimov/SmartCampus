using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Homework
{
    [Authorize(Roles = "Admin,Teacher")]
    public class EditModel : PageModel
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public EditModel(
            IHomeworkService homeworkService,
            IGroupService groupService,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _homeworkService = homeworkService;
            _groupService = groupService;
            _teacherService = teacherService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        [BindProperty]
        public HomeworkDto Homework { get; set; } = new();

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var homework = await _homeworkService.GetHomeworkByIdAsync(id);

            if (homework == null)
            {
                return NotFound();
            }


            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher == null || homework.TeacherId != teacher.Id)
                    return Unauthorized();
            }

            Homework = homework;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
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
                await OnGetAsync(Homework.Id);
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
                    var existingHomework = await _homeworkService.GetHomeworkByIdAsync(Homework.Id);

                    if (teacher == null || existingHomework == null || existingHomework.TeacherId != teacher.Id)
                        return Unauthorized();
                }

                await _homeworkService.UpdateHomeworkAsync(Homework);
                ToastHelper.ShowSuccess(this, "Homework updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating homework: {ex.Message}");
                await OnGetAsync(Homework.Id);
                return Page();
            }
        }
    }
}