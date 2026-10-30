using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace parkEASE.Models
{
    public class Transaction
    {
        [Key]
        public required string TransactionID { get; set; }
        public required string PlateNumber { get; set; }
        public required string VehicleType { get; set; }

        public int SlotID { get; set; }
        [ForeignKey("SlotID")]
        public ParkingSlot? ParkingSlot { get; set; }

        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public int ParkingDuration { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeDue { get; set; }
        public required string PaymentMethod { get; set; } // Cash or GCash
        public string PaymentStatus { get; set; } = "Paid";

        public int? ProcessedBy { get; set; }
        [ForeignKey("ProcessedBy")]
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}