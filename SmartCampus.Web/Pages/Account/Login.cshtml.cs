using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Data.Entities;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email Address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string? Password { get; set; }

            [Display(Name = "Remember me for 7 days")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return LocalRedirect("/");
            }

            await HttpContext.SignOutAsync();

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            returnUrl ??= "/";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(Input.Email) || string.IsNullOrEmpty(Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return Page();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(Input.Email.Trim());

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        _logger.LogWarning("Inactive user login attempt for email '{Email}'.", Input.Email);
                        ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                        return Page();
                    }

                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName ?? "",
                        Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User '{UserName}' logged in successfully at {LoginTime}.", user.UserName, DateTime.UtcNow);

                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var fullNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

                        if (fullNameClaim == null)
                        {
                            var claim = new System.Security.Claims.Claim("FullName", user.FullName ?? user.UserName ?? "User");
                            await _userManager.AddClaimAsync(user, claim);
                            await _signInManager.RefreshSignInAsync(user);
                        }

                        return LocalRedirect(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account '{UserName}' is locked out at {LockoutTime}.", user.UserName, DateTime.UtcNow);
                        ToastHelper.ShowWarning(this, "Your account has been temporarily locked due to multiple failed login attempts. Please try again later.");
                        return Page();
                    }

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }

                    _logger.LogWarning("Invalid login attempt for user '{UserName}' at {AttemptTime}.", Input.Email, DateTime.UtcNow);
                    ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
                }
                else
                {
                    _logger.LogWarning("Login attempt with non-existent email '{Email}' at {AttemptTime}.", Input.Email, DateTime.UtcNow);
                    ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email '{Email}'.", Input.Email);
                ModelState.AddModelError(string.Empty, "An error occurred while trying to log in. Please try again.");
            }

            return Page();
        }
    }
}