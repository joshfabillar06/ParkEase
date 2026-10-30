using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;
using System.Security.Claims;

namespace parkEASE.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public ProfileModel(ParkEaseDbContext context)
        {
            _context = context;
        }

        public User? CurrentUser { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadUserDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            await LoadUserDataAsync();

            if (CurrentUser == null) return RedirectToPage("/Login");

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "New password and confirmation do not match.";
                return Page();
            }

            if (CurrentUser.PasswordHash != CurrentPassword)
            {
                ErrorMessage = "Incorrect current password.";
                return Page();
            }

            CurrentUser.PasswordHash = NewPassword;
            await _context.SaveChangesAsync();

            StatusMessage = "Password updated successfully.";
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }

        private async Task LoadUserDataAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                CurrentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            }
        }
    }
}