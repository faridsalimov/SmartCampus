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
    public class CreateModel : PageModel
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public CreateModel(
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

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                RedirectToPage("/Account/Login");
                return;
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
                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                    if (teacher != null)
                    {
                        var teacherGroupIds = await _userContextHelper.GetTeacherGroupIdsAsync(user.Id);
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);


                Guid? teacherId = null;
                if (userRoles.Contains("Teacher"))
                {
                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                    teacherId = teacher?.Id;



                }
                else if (userRoles.Contains("Admin"))
                {

                    if (Homework.TeacherId == Guid.Empty)
                    {
                        ModelState.AddModelError("Homework.TeacherId", "Please select a teacher for this homework.");
                        await OnGetAsync();
                        return Page();
                    }
                    teacherId = Homework.TeacherId;
                }

                if (!teacherId.HasValue || teacherId == Guid.Empty)
                {
                    ModelState.AddModelError(string.Empty, "Invalid teacher. Please ensure you are a valid teacher in the system.");
                    await OnGetAsync();
                    return Page();
                }

                Homework.TeacherId = teacherId.Value;
                await _homeworkService.CreateHomeworkAsync(Homework);
                ToastHelper.ShowSuccess(this, "Homework created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating homework: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}