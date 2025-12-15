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
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public DetailsModel(
            ILessonService lessonService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _lessonService = lessonService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        public LessonDto? Lesson { get; set; }
        public bool IsTeacher { get; set; }
        public bool IsOwner { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Lesson = await _lessonService.GetLessonByIdAsync(id);

            if (Lesson == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            IsTeacher = userRoles.Contains("Teacher");


            if (IsTeacher && user != null)
            {
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (teacherId.HasValue)
                {
                    var isAuthorized = await _userContextHelper.IsTeacherTeachingGroupAsync(teacherId.Value, Lesson.GroupId);
                    IsOwner = Lesson.TeacherId == teacherId;

                    if (!isAuthorized && !IsOwner)
                        return Unauthorized();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (userRoles.Contains("Teacher") && user != null)
                {
                    var lesson = await _lessonService.GetLessonByIdAsync(id);
                    if (lesson != null)
                    {
                        var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                        if (teacherId.HasValue)
                        {
                            if (lesson.TeacherId != teacherId && !userRoles.Contains("Admin"))
                            {
                                return Unauthorized();
                            }
                        }
                    }
                }
                else if (!userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                await _lessonService.DeleteLessonAsync(id);
                ToastHelper.ShowSuccess(this, "Lesson deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting lesson: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}