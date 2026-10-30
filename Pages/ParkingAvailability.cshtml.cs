using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;

namespace parkEASE.Pages
{
    public class ParkingAvailabilityModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public ParkingAvailabilityModel(ParkEaseDbContext context) => _context = context;

        public List<ParkingSlotCard> Slots { get; set; } = new();
        public List<ParkingSlot> ArchivedSlots { get; set; } = new();
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var slots = await _context.ParkingSlots.AsNoTracking()
                .Where(s => !s.IsArchived)
                .OrderBy(s => s.SlotID)
                .ToListAsync();

            var activeTransactions = await _context.Transactions.AsNoTracking()
                .Where(t => t.PaymentStatus == "Parked")
                .ToListAsync();

            Slots = slots.Select(slot => new ParkingSlotCard
            {
                Slot = slot,
                CurrentVehicle = activeTransactions
                    .Where(t => t.SlotID == slot.SlotID)
                    .OrderByDescending(t => t.EntryTime)
                    .FirstOrDefault()
            }).ToList();

            ArchivedSlots = await _context.ParkingSlots.AsNoTracking()
                .Where(s => s.IsArchived)
                .OrderBy(s => s.SlotID)
                .ToListAsync();

            TotalSpaces = Slots.Count;
            OccupiedSpaces = Slots.Count(s => s.CurrentVehicle != null || s.Slot.Status == "Occupied");
            AvailableSpaces = Slots.Count - OccupiedSpaces;
            return Page();
        }

        public async Task<IActionResult> OnPostArchiveSlotAsync(int slotId)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.SlotID == slotId && !s.IsArchived);
            if (slot == null) return NotFound();

            bool hasActiveVehicle = await _context.Transactions.AnyAsync(t =>
                t.SlotID == slotId && t.PaymentStatus == "Parked");

            if (hasActiveVehicle || slot.Status == "Occupied")
            {
                TempData["SlotMessage"] = "This slot cannot be archived while a vehicle is occupying it.";
                return RedirectToPage();
            }

            slot.IsArchived = true;
            slot.Status = "Available";
            await _context.SaveChangesAsync();
            TempData["SlotMessage"] = $"{slot.SlotName} was archived. It can be restored later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreSlotAsync(int slotId)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.SlotID == slotId && s.IsArchived);
            if (slot == null) return NotFound();

            slot.IsArchived = false;
            slot.Status = "Available";
            await _context.SaveChangesAsync();
            TempData["SlotMessage"] = $"{slot.SlotName} was restored and is available.";
            return RedirectToPage();
        }
    }

    public class ParkingSlotCard
    {
        public ParkingSlot Slot { get; set; } = null!;
        public Transaction? CurrentVehicle { get; set; }
    }
}
