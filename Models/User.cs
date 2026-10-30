using System.ComponentModel.DataAnnotations;

namespace parkEASE.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public required string EmployeeID { get; set; }
        public required string FullName { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; } // Admin or Staff
        public string AccountStatus { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}