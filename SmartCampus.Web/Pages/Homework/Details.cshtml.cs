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
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IStudentService _studentService;
        private readonly IHomeworkSubmissionService _homeworkSubmissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserContextHelper _userContextHelper;
        private readonly IWebHostEnvironment _environment;

        public DetailsModel(
            IHomeworkService homeworkService,
            IStudentService studentService,
            IHomeworkSubmissionService homeworkSubmissionService,
            UserManager<ApplicationUser> userManager,
            UserContextHelper userContextHelper,
            IWebHostEnvironment environment)
        {
            _homeworkService = homeworkService;
            _studentService = studentService;
            _homeworkSubmissionService = homeworkSubmissionService;
            _userManager = userManager;
            _userContextHelper = userContextHelper;
            _environment = environment;
        }

        public HomeworkDto? Homework { get; set; }
        public bool IsStudent { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTeacher { get; set; }
        public bool IsOwner { get; set; }
        public HomeworkSubmissionDto? StudentSubmission { get; set; }
        public IEnumerable<HomeworkSubmissionDto> StudentSubmissions { get; set; } = new List<HomeworkSubmissionDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Homework = await _homeworkService.GetHomeworkByIdAsync(id);

            if (Homework == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            IsStudent = userRoles.Contains("Student");
            IsTeacher = userRoles.Contains("Teacher");


            if (IsStudent && user != null)
            {
                var studentGroupId = await _userContextHelper.GetStudentGroupIdAsync(user.Id);
                if (studentGroupId != Homework.GroupId)
                {
                    return Unauthorized();
                }
                IsCompleted = Homework.IsCompleted;


                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student != null)
                {
                    StudentSubmission = await _homeworkSubmissionService.GetStudentSubmissionAsync(id, student.Id);
                }
            }


            if (IsTeacher && user != null)
            {
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                if (teacherId.HasValue)
                {
                    var isAuthorized = await _userContextHelper.IsTeacherTeachingGroupAsync(teacherId.Value, Homework.GroupId);
                    IsOwner = Homework.TeacherId == teacherId;

                    if (!isAuthorized && !IsOwner)
                        return Unauthorized();


                    StudentSubmissions = await _homeworkSubmissionService.GetSubmissionsByHomeworkAsync(id);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Student"))
                    return Unauthorized();

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student == null)
                    return Unauthorized();

                var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                if (homework == null)
                    return NotFound();


                var studentGroupId = await _userContextHelper.GetStudentGroupIdAsync(user.Id);
                if (studentGroupId != homework.GroupId)
                    return Unauthorized();

                string? fileUrl = null;
                string? submissionText = Request.Form["submissionText"];


                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {

                        if (file.Length > 25 * 1024 * 1024)
                        {
                            ToastHelper.ShowError(this, "File size cannot exceed 25MB");
                            return RedirectToPage();
                        }


                        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "submissions", id.ToString());
                        Directory.CreateDirectory(uploadsDir);


                        var fileName = $"{student.Id}_{DateTime.UtcNow.Ticks}_{file.FileName}";
                        var filePath = Path.Combine(uploadsDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        fileUrl = $"/uploads/submissions/{id}/{fileName}";
                    }
                }


                if (string.IsNullOrWhiteSpace(submissionText) && string.IsNullOrWhiteSpace(fileUrl))
                {
                    ToastHelper.ShowError(this, "Please either upload a file or write a response");
                    return RedirectToPage();
                }


                var submission = new HomeworkSubmissionDto
                {
                    HomeworkId = id,
                    StudentId = student.Id,
                    SubmissionText = submissionText,
                    FileUrl = fileUrl,
                    SubmissionDate = DateTime.UtcNow,
                    Status = "Submitted",
                    IsLate = homework.DueDate < DateTime.UtcNow
                };

                await _homeworkSubmissionService.CreateSubmissionAsync(submission);

                ToastHelper.ShowSuccess(this, "Homework submitted successfully!");
                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error submitting homework: {ex.Message}");
                return RedirectToPage(new { id = id });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);


                if (userRoles.Contains("Teacher") && user != null)
                {
                    var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);
                    if (teacherId.HasValue)
                    {
                        var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                        if (homework?.TeacherId != teacherId && !userRoles.Contains("Admin"))
                        {
                            return Unauthorized();
                        }
                    }
                }
                else if (!userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                await _homeworkService.DeleteHomeworkAsync(id);
                ToastHelper.ShowSuccess(this, "Homework deleted successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting homework: {ex.Message}");
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostCompleteAsync(Guid id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Student"))
                {
                    return Unauthorized();
                }

                var student = await _studentService.GetStudentByApplicationUserIdAsync(user.Id);
                if (student == null)
                {
                    return Unauthorized();
                }

                await _homeworkService.MarkHomeworkCompletedByStudentAsync(id, student.Id);
                ToastHelper.ShowSuccess(this, "Homework marked as completed.");
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error marking homework as completed: {ex.Message}");
            }

            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostGradeSubmissionAsync(Guid id, Guid submissionId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Teacher"))
                    return Unauthorized();

                var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);

                if (homework?.TeacherId != teacherId && !userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                // For now, redirect with message - full grading UI can be implemented later
                ToastHelper.ShowInfo(this, "Grading feature coming soon. Please use the admin panel for detailed grading.");
                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error with grading: {ex.Message}");
                return RedirectToPage(new { id = id });
            }
        }

        public async Task<IActionResult> OnPostDeleteSubmissionAsync(Guid id, Guid submissionId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Teacher"))
                    return Unauthorized();


                var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);

                if (homework?.TeacherId != teacherId && !userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                await _homeworkSubmissionService.DeleteSubmissionAsync(submissionId);
                ToastHelper.ShowSuccess(this, "Submission deleted successfully!");
                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error deleting submission: {ex.Message}");
                return RedirectToPage(new { id = id });
            }
        }

        public async Task<IActionResult> OnPostRejectSubmissionAsync(Guid id, Guid submissionId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Teacher"))
                    return Unauthorized();

                var homework = await _homeworkService.GetHomeworkByIdAsync(id);
                var teacherId = await _userContextHelper.GetTeacherIdByUserIdAsync(user.Id);

                if (homework?.TeacherId != teacherId && !userRoles.Contains("Admin"))
                {
                    return Unauthorized();
                }

                await _homeworkSubmissionService.UpdateSubmissionStatusAsync(submissionId, "Rejected");
                ToastHelper.ShowSuccess(this, "Submission rejected successfully!");
                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ToastHelper.ShowError(this, $"Error rejecting submission: {ex.Message}");
                return RedirectToPage(new { id = id });
            }
        }
    }
}