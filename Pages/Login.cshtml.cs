using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;

namespace parkEASE.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public LoginModel(ParkEaseDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Find user matching username/email and password
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == Password && u.AccountStatus == "Active");

            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Create claims for logged-in user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("EmployeeID", user.EmployeeID)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Index");
        }
    }
}