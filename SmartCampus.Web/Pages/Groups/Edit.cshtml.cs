using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SmartCampus.Core.DTOs;
using SmartCampus.Services.Interfaces;
using SmartCampus.Web.Utilities;

namespace SmartCampus.Web.Pages.Groups
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IGroupService _groupService;

        public EditModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [BindProperty]
        public GroupDto Group { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);

            if (group == null)
            {
                return NotFound();
            }

            Group = group;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _groupService.UpdateGroupAsync(Group);
                ToastHelper.ShowSuccess(this, "Group updated successfully.");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating group: {ex.Message}");
                return Page();
            }
        }
    }
}