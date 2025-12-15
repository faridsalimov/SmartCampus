using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Groups
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IGroupService _groupService;

        public CreateModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [BindProperty]
        public GroupDto Group { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _groupService.CreateGroupAsync(Group);
                ToastHelper.ShowSuccess(this, "Group created successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating group: {ex.Message}");
                return Page();
            }
        }
    }
}