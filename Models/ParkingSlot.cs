using System.ComponentModel.DataAnnotations;

namespace parkEASE.Models
{
    public class ParkingSlot
    {
        [Key]
        public int SlotID { get; set; }

        [Required]
        public required string SlotName { get; set; }

        public string Status { get; set; } = "Available";

        // Archived slots are hidden from live capacity and cannot be assigned by the POS.
        public bool IsArchived { get; set; } = false;
    }
}
