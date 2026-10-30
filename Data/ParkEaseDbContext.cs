using Microsoft.EntityFrameworkCore;
using parkEASE.Models;

namespace parkEASE.Data
{
    public class ParkEaseDbContext : DbContext
    {
        public ParkEaseDbContext(DbContextOptions<ParkEaseDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ParkingRate> ParkingRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ParkingSlot>().Property(s => s.IsArchived).HasDefaultValue(false);
        }
    }
}
