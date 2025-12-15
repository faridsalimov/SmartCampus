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
    public class IndexModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public IndexModel(
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

        public IList<LessonDto> Lessons { get; set; } = new List<LessonDto>();
        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();
        public Guid? FilterGroupId { get; set; }
        public string? SearchTerm { get; set; }
        public string? UserRole { get; set; }

        public async Task OnGetAsync(string? searchTerm = null, Guid? filterGroupId = null)
        {
            SearchTerm = searchTerm;
            FilterGroupId = filterGroupId;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var userRoles = await _userManager.GetRolesAsync(user);
            UserRole = userRoles.FirstOrDefault();
            IEnumerable<LessonDto> lessons = new List<LessonDto>();


            if (userRoles.Contains("Student"))
            {

                var studentGroupId = await _userContextHelper.GetStudentGroupIdAsync(user.Id);
                if (studentGroupId.HasValue)
                {
                    lessons = await _lessonService.GetLessonsByGroupAsync(studentGroupId.Value);
                    Groups = new List<GroupDto> { (await _groupService.GetGroupByIdAsync(studentGroupId.Value))! };
                }
                else
                {
                    lessons = new List<LessonDto>();
                }
            }
            else if (userRoles.Contains("Teacher"))
            {

                var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                if (teacher != null)
                {
                    lessons = await _lessonService.GetLessonsByTeacherAsync(teacher.Id);


                    var teacherGroupIds = lessons
                        .Select(l => l.GroupId)
                        .Distinct()
                        .ToList();

                    if (teacherGroupIds.Count > 0)
                    {
                        var allGroups = await _groupService.GetAllGroupsAsync();
                        Groups = allGroups.Where(g => teacherGroupIds.Contains(g.Id)).ToList();
                    }
                }
            }
            else if (userRoles.Contains("Admin"))
            {

                lessons = await _lessonService.GetAllLessonsAsync();
                Groups = (await _groupService.GetAllGroupsAsync()).ToList();
            }


            if (filterGroupId.HasValue && filterGroupId != Guid.Empty)
            {
                lessons = lessons.Where(l => l.GroupId == filterGroupId).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                lessons = lessons.Where(l =>
                    (l.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Lessons = lessons.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (!userRoles.Contains("Admin") && !userRoles.Contains("Teacher"))
                {
                    return Unauthorized();
                }


                if (userRoles.Contains("Teacher") && user != null)
                {
                    var lesson = await _lessonService.GetLessonByIdAsync(id);
                    if (lesson != null)
                    {
                        var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                        if (teacher != null)
                        {
                            var isAuthorized = await _userContextHelper.IsTeacherTeachingGroupAsync(teacher.Id, lesson.GroupId);
                            if (!isAuthorized)
                                return Unauthorized();
                        }
                    }
                }

                await _lessonService.DeleteLessonAsync(id);
                ToastHelper.ShowSuccess(this, "Lesson deleted successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting lesson: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}