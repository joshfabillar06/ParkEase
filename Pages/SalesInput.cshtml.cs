using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;
using System.Security.Claims;

namespace parkEASE.Pages
{
    public class SalesInputModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public parkEASE.Models.ParkingRate CurrentRates { get; set; } = new();

        public SalesInputModel(ParkEaseDbContext context)
        {
            _context = context;
        }

        public string? AssignedSlotName { get; set; }
        public int AvailableSlotCount { get; set; }
        public List<Transaction> ActiveParkedVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentRates = await _context.ParkingRates.FirstOrDefaultAsync(r => r.RateID == 1)
                           ?? new parkEASE.Models.ParkingRate { SedanRate = 50, SuvRate = 75, VanRate = 100, MotorcycleRate = 30 };

            var nextSlot = await _context.ParkingSlots
                .FirstOrDefaultAsync(s => s.Status == "Available" && !s.IsArchived);

            AvailableSlotCount = await _context.ParkingSlots.CountAsync(s => s.Status == "Available" && !s.IsArchived);
            AssignedSlotName = nextSlot?.SlotName ?? "FULL";

            ActiveParkedVehicles = await _context.Transactions
                .Include(t => t.ParkingSlot)
                .Where(t => t.PaymentStatus == "Parked")
                .OrderBy(t => t.EntryTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCheckInAsync([FromBody] CheckInInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PlateNumber))
            {
                return BadRequest(new { success = false, message = "Invalid check-in payload." });
            }

            string plate = model.PlateNumber.Trim().ToUpper();

            bool alreadyParked = await _context.Transactions
                .AnyAsync(t => t.PaymentStatus == "Parked" && t.PlateNumber == plate);

            if (alreadyParked)
            {
                return BadRequest(new { success = false, message = "This vehicle is already checked in and has not exited yet." });
            }

            var slot = await _context.ParkingSlots
                .FirstOrDefaultAsync(s => s.Status == "Available" && !s.IsArchived);

            if (slot == null)
            {
                return BadRequest(new { success = false, message = "No parking slots currently available." });
            }

            int? currentUserId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int parsedId))
            {
                currentUserId = parsedId;
            }

            string transactionId = "TKT-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var dbRates = await _context.ParkingRates.FirstOrDefaultAsync(r => r.RateID == 1);
            decimal hourlyRate = model.VehicleType switch
            {
                "Sedan" => dbRates?.SedanRate ?? 50.00m,
                "SUV" => dbRates?.SuvRate ?? 75.00m,
                "Van" => dbRates?.VanRate ?? 100.00m,
                "Motorcycle" => dbRates?.MotorcycleRate ?? 30.00m,
                _ => 50.00m
            };

            var entryTime = DateTime.Now;

            var transaction = new Transaction
            {
                TransactionID = transactionId,
                PlateNumber = plate,
                VehicleType = model.VehicleType,
                SlotID = slot.SlotID,
                EntryTime = entryTime,
                ExitTime = null,
                ParkingDuration = 0,
                HourlyRate = hourlyRate,
                TotalAmount = 0,
                AmountReceived = 0,
                ChangeDue = 0,
                PaymentMethod = "Pending",
                PaymentStatus = "Parked",
                ProcessedBy = currentUserId,
                CreatedAt = entryTime
            };

            slot.Status = "Occupied";

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                transactionId = transaction.TransactionID,
                plateNumber = transaction.PlateNumber,
                vehicleType = transaction.VehicleType,
                assignedSlot = slot.SlotName,
                entryTime = transaction.EntryTime.ToString("hh:mm tt"),
                date = transaction.EntryTime.ToString("MMMM dd, yyyy")
            });
        }

        public async Task<IActionResult> OnPostFindActiveAsync([FromBody] FindActiveInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Query))
            {
                return BadRequest(new { success = false, message = "Enter a plate number or ticket ID." });
            }

            string query = model.Query.Trim().ToUpper();

            var transaction = await _context.Transactions
                .Include(t => t.ParkingSlot)
                .Where(t => t.PaymentStatus == "Parked" && (t.TransactionID == query || t.PlateNumber == query))
                .OrderByDescending(t => t.EntryTime)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return BadRequest(new { success = false, message = "No active parked vehicle found for that plate or ticket ID." });
            }

            var (hours, amountDue) = CalculateFee(transaction.EntryTime, DateTime.Now, transaction.HourlyRate);

            return new JsonResult(new
            {
                success = true,
                transactionId = transaction.TransactionID,
                plateNumber = transaction.PlateNumber,
                vehicleType = transaction.VehicleType,
                assignedSlot = transaction.ParkingSlot?.SlotName ?? "P--",
                entryTime = transaction.EntryTime.ToString("MMM dd, yyyy hh:mm tt"),
                hourlyRate = transaction.HourlyRate,
                estimatedHours = hours,
                amountDue = amountDue
            });
        }

        public async Task<IActionResult> OnPostCheckOutAsync([FromBody] CheckOutInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TransactionID))
            {
                return BadRequest(new { success = false, message = "Invalid check-out payload." });
            }

            var transaction = await _context.Transactions
                .Include(t => t.ParkingSlot)
                .FirstOrDefaultAsync(t => t.TransactionID == model.TransactionID && t.PaymentStatus == "Parked");

            if (transaction == null)
            {
                return BadRequest(new { success = false, message = "This vehicle has already exited or does not exist." });
            }

            var exitTime = DateTime.Now;
            var (hours, totalAmount) = CalculateFee(transaction.EntryTime, exitTime, transaction.HourlyRate);

            if (model.AmountReceived < totalAmount)
            {
                return BadRequest(new { success = false, message = "Amount received is less than the total fee due." });
            }

            decimal changeDue = model.AmountReceived - totalAmount;

            transaction.ExitTime = exitTime;
            transaction.ParkingDuration = hours;
            transaction.TotalAmount = totalAmount;
            transaction.AmountReceived = model.AmountReceived;
            transaction.ChangeDue = changeDue;
            transaction.PaymentMethod = model.PaymentMethod;
            transaction.PaymentStatus = "Paid";

            if (transaction.ParkingSlot != null)
            {
                transaction.ParkingSlot.Status = "Available";
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                transactionId = transaction.TransactionID,
                plateNumber = transaction.PlateNumber,
                vehicleType = transaction.VehicleType,
                assignedSlot = transaction.ParkingSlot?.SlotName ?? "P--",
                entryTime = transaction.EntryTime.ToString("MMM dd, yyyy hh:mm tt"),
                exitTime = transaction.ExitTime.Value.ToString("MMM dd, yyyy hh:mm tt"),
                parkingDuration = transaction.ParkingDuration,
                amountPaid = transaction.TotalAmount,
                amountReceived = transaction.AmountReceived,
                changeDue = transaction.ChangeDue,
                date = exitTime.ToString("MMMM dd, yyyy")
            });
        }

        private static (int hours, decimal amount) CalculateFee(DateTime entryTime, DateTime exitTime, decimal hourlyRate)
        {
            double elapsedHours = (exitTime - entryTime).TotalHours;
            int hours = (int)Math.Max(1, Math.Ceiling(elapsedHours));
            return (hours, hours * hourlyRate);
        }
    }

    public class CheckInInputModel
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = "Sedan";
    }

    public class FindActiveInputModel
    {
        public string Query { get; set; } = string.Empty;
    }

    public class CheckOutInputModel
    {
        public string TransactionID { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public decimal AmountReceived { get; set; }
    }
}
