using System.ComponentModel.DataAnnotations;

namespace parkEASE.Models
{
    public class ParkingRate
    {
        [Key]
        public int RateID { get; set; }
        public decimal SedanRate { get; set; }
        public decimal SuvRate { get; set; }
        public decimal VanRate { get; set; }
        public decimal MotorcycleRate { get; set; }
    }
}