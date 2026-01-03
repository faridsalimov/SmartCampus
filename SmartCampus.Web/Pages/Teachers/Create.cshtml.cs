using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Teachers
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _userManager = userManager;
        }

        [BindProperty]
        public TeacherDto Teacher { get; set; } = new();

        public async Task OnGetAsync()
        {
            await Task.CompletedTask;
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
                var existingUser = await _userManager.FindByEmailAsync(Teacher.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Teacher.Email", "An account with this email already exists.");
                    await OnGetAsync();
                    return Page();
                }

                var applicationUser = new ApplicationUser
                {
                    UserName = Teacher.Email,
                    Email = Teacher.Email,
                    FullName = Teacher.FullName,
                    PhoneNumber = Teacher.PhoneNumber,
                    UserRole = "Teacher",
                    IsActive = true
                };

                var createUserResult = await _userManager.CreateAsync(applicationUser, Teacher.Password);
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await OnGetAsync();
                    return Page();
                }

                var assignRoleResult = await _userManager.AddToRoleAsync(applicationUser, "Teacher");
                if (!assignRoleResult.Succeeded)
                {
                    foreach (var error in assignRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await OnGetAsync();
                    return Page();
                }

                Teacher.ApplicationUserId = applicationUser.Id;
                await _teacherService.CreateTeacherAsync(Teacher);
                ToastHelper.ShowSuccess(this, "Teacher created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating teacher: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}