using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Students
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            IStudentService studentService,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager)
        {
            _studentService = studentService;
            _groupService = groupService;
            _userManager = userManager;
        }

        [BindProperty]
        public StudentDto Student { get; set; } = new();

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public async Task OnGetAsync()
        {
            Groups = (await _groupService.GetAllGroupsAsync()).ToList();
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
                if (Student.GroupId == Guid.Empty)
                {
                    ModelState.AddModelError("Student.GroupId", "Group is required.");
                    await OnGetAsync();
                    return Page();
                }

                var existingUser = await _userManager.FindByEmailAsync(Student.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Student.Email", "An account with this email already exists.");
                    await OnGetAsync();
                    return Page();
                }

                var applicationUser = new ApplicationUser
                {
                    UserName = Student.Email,
                    Email = Student.Email,
                    FullName = Student.FullName,
                    PhoneNumber = Student.PhoneNumber,
                    UserRole = "Student",
                    IsActive = true
                };

                var createUserResult = await _userManager.CreateAsync(applicationUser, Student.Password);
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await OnGetAsync();
                    return Page();
                }

                var assignRoleResult = await _userManager.AddToRoleAsync(applicationUser, "Student");
                if (!assignRoleResult.Succeeded)
                {
                    foreach (var error in assignRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await OnGetAsync();
                    return Page();
                }

                Student.ApplicationUserId = applicationUser.Id;
                await _studentService.CreateStudentAsync(Student);
                ToastHelper.ShowSuccess(this, "Student created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating student: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}