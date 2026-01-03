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
    public class CreateModel : PageModel
    {
        private readonly ILessonService _lessonService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;

        public CreateModel(
            ILessonService lessonService,
            IGroupService groupService,
            ITeacherService teacherService,
            IAttendanceService attendanceService,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper)
        {
            _lessonService = lessonService;
            _groupService = groupService;
            _teacherService = teacherService;
            _attendanceService = attendanceService;
            _studentService = studentService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
        }

        [BindProperty]
        public LessonDto Lesson { get; set; } = new();

        public IList<GroupDto> Groups { get; set; } = new List<GroupDto>();
        public IList<StudentDto> SelectedGroupStudents { get; set; } = new List<StudentDto>();

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


                if (userRoles.Contains("Teacher"))
                {
                    var teacher = await _teacherService.GetTeacherByApplicationUserIdAsync(user.Id);
                    if (teacher == null)
                        return Unauthorized();

                    Lesson.TeacherId = teacher.Id;
                }
                else if (userRoles.Contains("Admin"))
                {

                    if (Lesson.TeacherId == Guid.Empty)
                    {
                        ModelState.AddModelError(string.Empty, "Please select a teacher for this lesson.");
                        await OnGetAsync();
                        return Page();
                    }
                }

                var createdLesson = await _lessonService.CreateLessonAsync(Lesson);
                
                try
                {
                    await _attendanceService.CreateBulkAttendanceForLessonAsync(createdLesson.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not create attendance records: {ex.Message}");
                }

                ToastHelper.ShowSuccess(this, "Lesson created successfully with attendance records.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating lesson: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetStudentsByGroupAsync(Guid groupId)
        {
            try
            {
                var students = await _studentService.GetStudentsByGroupAsync(groupId);
                return new JsonResult(students.ToList());
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }
    }
}