using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;

namespace parkEASE.Pages
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public SettingsModel(ParkEaseDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ParkingRate Rates { get; set; } = new();

        [BindProperty]
        public NewAccountInput NewAccount { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string AccountStatusMessage { get; set; }

        public string AccountErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch dynamic rates from database
            var currentRates = await _context.ParkingRates.FirstOrDefaultAsync(r => r.RateID == 1);
            if (currentRates != null)
            {
                Rates.SedanRate = currentRates.SedanRate;
                Rates.SuvRate = currentRates.SuvRate;
                Rates.VanRate = currentRates.VanRate;
                Rates.MotorcycleRate = currentRates.MotorcycleRate;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveRatesAsync()
        {
            // Only allow Admins to save rate changes
            if (!User.IsInRole("Admin")) return Forbid();

            // Save the new rates to the database
            var currentRates = await _context.ParkingRates.FirstOrDefaultAsync(r => r.RateID == 1);
            if (currentRates != null)
            {
                currentRates.SedanRate = Rates.SedanRate;
                currentRates.SuvRate = Rates.SuvRate;
                currentRates.VanRate = Rates.VanRate;
                currentRates.MotorcycleRate = Rates.MotorcycleRate;

                await _context.SaveChangesAsync();
                StatusMessage = "Parking rates updated successfully. Changes are now live in the POS.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAccountAsync()
        {
            // Only allow Admins to create new accounts
            if (!User.IsInRole("Admin")) return Forbid();

            if (NewAccount.Password != NewAccount.ConfirmPassword)
            {
                AccountErrorMessage = "Password and confirmation do not match.";
                return Page();
            }

            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == NewAccount.Username || u.EmployeeID == NewAccount.EmployeeID);

            if (exists)
            {
                AccountErrorMessage = "An account with that username or employee ID already exists.";
                return Page();
            }

            var user = new User
            {
                EmployeeID = NewAccount.EmployeeID,
                FullName = NewAccount.FullName,
                Username = NewAccount.Username,
                PasswordHash = NewAccount.Password,
                Role = NewAccount.Role,
                AccountStatus = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            AccountStatusMessage = $"{NewAccount.Role} account for \"{NewAccount.FullName}\" created successfully.";

            return RedirectToPage();
        }
    }

    public class NewAccountInput
    {
        public string FullName { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
