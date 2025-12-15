using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Data.Entities;

namespace SmartCampus.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ProfileModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public string? StatusMessage { get; set; }
        public bool IsStatusSuccess { get; set; }

        [BindProperty]
        public ProfileInputModel? Input { get; set; }

        [BindProperty]
        public ChangePasswordInput? PasswordInput { get; set; }

        [BindProperty]
        public IFormFile? AvatarFile { get; set; }

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
            [Display(Name = "First Name")]
            public string? FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
            [Display(Name = "Last Name")]
            public string? LastName { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email Address")]
            public string? Email { get; set; }

            [Phone(ErrorMessage = "Invalid phone number")]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
            [Display(Name = "Bio")]
            public string? Bio { get; set; }
        }

        public class ChangePasswordInput
        {
            [Required(ErrorMessage = "Current password is required")]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string? OldPassword { get; set; }

            [Required(ErrorMessage = "New password is required")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} and at max {1} characters long", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
            public string? ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unable to load user ID from claims");
                return NotFound($"Unable to load user.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID '{UserId}' not found", userId);
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            CurrentUser = user;

            Input = new ProfileInputModel
            {
                FirstName = user.FullName?.Split().FirstOrDefault() ?? "",
                LastName = user.FullName?.Split().Skip(1).FirstOrDefault() ?? "",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            PasswordInput = new ChangePasswordInput();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                if (Input?.FirstName != null && Input.LastName != null)
                {
                    user.FullName = $"{Input.FirstName} {Input.LastName}".Trim();
                }

                if (!string.IsNullOrEmpty(Input?.Email) && Input.Email != user.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        StatusMessage = "Error: Failed to update email";
                        IsStatusSuccess = false;
                        return await OnGetAsync();
                    }
                    user.UserName = Input.Email.Split()[0];
                }

                if (!string.IsNullOrEmpty(Input?.PhoneNumber))
                {
                    user.PhoneNumber = Input.PhoneNumber;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    StatusMessage = "Your profile has been updated successfully!";
                    IsStatusSuccess = true;
                    _logger.LogInformation("User '{UserName}' updated their profile", user.UserName);
                }
                else
                {
                    StatusMessage = "Error: Failed to update profile";
                    IsStatusSuccess = false;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user '{UserId}'", userId);
                StatusMessage = "An error occurred while updating your profile";
                IsStatusSuccess = false;
            }

            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user,
                    PasswordInput?.OldPassword ?? "",
                    PasswordInput?.NewPassword ?? "");

                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    StatusMessage = "Your password has been changed successfully!";
                    IsStatusSuccess = true;
                    _logger.LogInformation("User '{UserName}' changed their password", user.UserName);
                }
                else
                {
                    StatusMessage = "Error: Failed to change password";
                    IsStatusSuccess = false;
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user '{UserId}'", userId);
                StatusMessage = "An error occurred while changing your password";
                IsStatusSuccess = false;
            }

            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("User '{UserName}' deleted their account", user.UserName);
                    return RedirectToPage("/Index");
                }
                else
                {
                    StatusMessage = "Error: Failed to delete account";
                    IsStatusSuccess = false;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user '{UserId}'", userId);
                StatusMessage = "An error occurred while deleting your account";
                IsStatusSuccess = false;
            }

            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync()
        {
            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                StatusMessage = "Error: Please select a file";
                IsStatusSuccess = false;
                return await OnGetAsync();
            }

            if (AvatarFile.Length > 5 * 1024 * 1024)
            {
                StatusMessage = "Error: File size must be less than 5MB";
                IsStatusSuccess = false;
                return await OnGetAsync();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                StatusMessage = "Error: Invalid file type. Only JPG, PNG, and GIF are allowed";
                IsStatusSuccess = false;
                return await OnGetAsync();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var uploadsDir = Path.Combine("wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                if (!string.IsNullOrEmpty(user.ProfilePhoto) && user.ProfilePhoto.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine("wwwroot", user.ProfilePhoto.TrimStart());
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                user.ProfilePhoto = $"/uploads/avatars/{fileName}";
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    var claims = await _userManager.GetClaimsAsync(user);
                    var profilePhotoClaim = claims.FirstOrDefault(c => c.Type == "ProfilePhoto");
                    if (profilePhotoClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, profilePhotoClaim);
                    }
                    if (!string.IsNullOrEmpty(user.ProfilePhoto))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("ProfilePhoto", user.ProfilePhoto));
                    }

                    StatusMessage = "Your avatar has been updated successfully!";
                    IsStatusSuccess = true;
                    _logger.LogInformation("User '{UserName}' updated their avatar", user.UserName);
                }
                else
                {
                    StatusMessage = "Error: Failed to update avatar";
                    IsStatusSuccess = false;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar for user '{UserId}'", userId);
                StatusMessage = "An error occurred while uploading your avatar";
                IsStatusSuccess = false;
            }

            return await OnGetAsync();
        }
    }
}