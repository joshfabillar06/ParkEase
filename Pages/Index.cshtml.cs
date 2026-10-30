using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;

namespace parkEASE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ParkEaseDbContext _context;

        public IndexModel(ParkEaseDbContext context) => _context = context;

        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public decimal TodaySales { get; set; }
        public decimal WeeklySales { get; set; }
        public decimal MonthlySales { get; set; }
        public decimal AverageWeeklySales { get; set; }
        public decimal AverageMonthlySales { get; set; }
        public double AverageStayHours { get; set; }
        public string PeakDay { get; set; } = "No data";
        public string PeakTime { get; set; } = "No data";
        public string MostParkedVehicleType { get; set; } = "No data";
        public int MostParkedVehicleCount { get; set; }
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<HourlyActivity> PeakHours { get; set; } = new();
        public List<DailySalesPoint> WeeklySalesChart { get; set; } = new();

        public async Task OnGetAsync()
        {
            var activeSlots = _context.ParkingSlots.AsNoTracking().Where(s => !s.IsArchived);
            TotalSpaces = await activeSlots.CountAsync();
            AvailableSpaces = await activeSlots.CountAsync(s => s.Status == "Available");
            OccupiedSpaces = await activeSlots.CountAsync(s => s.Status == "Occupied");

            var transactions = await _context.Transactions.AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var now = DateTime.Now;
            var today = now.Date;
            var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var paid = transactions.Where(t => t.PaymentStatus == "Paid" && t.ExitTime.HasValue).ToList();
            TodaySales = paid.Where(t => t.ExitTime!.Value.Date == today).Sum(t => t.TotalAmount);
            WeeklySales = paid.Where(t => t.ExitTime!.Value >= monday && t.ExitTime.Value < monday.AddDays(7)).Sum(t => t.TotalAmount);
            MonthlySales = paid.Where(t => t.ExitTime!.Value >= monthStart && t.ExitTime.Value < monthStart.AddMonths(1)).Sum(t => t.TotalAmount);

            // Average over the latest four calendar weeks and six calendar months, including zero-sales periods.
            var weekStarts = Enumerable.Range(0, 4).Select(i => monday.AddDays(-7 * i)).ToList();
            AverageWeeklySales = weekStarts.Select(start => paid
                .Where(t => t.ExitTime!.Value >= start && t.ExitTime.Value < start.AddDays(7))
                .Sum(t => t.TotalAmount)).DefaultIfEmpty(0).Average();

            var monthStarts = Enumerable.Range(0, 6)
                .Select(i => new DateTime(today.Year, today.Month, 1).AddMonths(-i)).ToList();
            AverageMonthlySales = monthStarts.Select(start => paid
                .Where(t => t.ExitTime!.Value >= start && t.ExitTime.Value < start.AddMonths(1))
                .Sum(t => t.TotalAmount)).DefaultIfEmpty(0).Average();

            var completed = paid.Where(t => t.ExitTime.HasValue && t.ExitTime.Value >= t.EntryTime).ToList();
            AverageStayHours = completed.Count == 0
                ? 0
                : completed.Average(t => (t.ExitTime!.Value - t.EntryTime).TotalHours);

            RecentTransactions = transactions.Take(7).ToList();

            var arrivals = transactions.ToList();
            for (var hour = 0; hour < 24; hour++)
            {
                PeakHours.Add(new HourlyActivity
                {
                    Hour = hour,
                    Label = DateTime.Today.AddHours(hour).ToString("h tt"),
                    Count = arrivals.Count(t => t.EntryTime.Hour == hour)
                });
            }

            var peakHour = PeakHours.OrderByDescending(h => h.Count).FirstOrDefault();
            if (peakHour != null && peakHour.Count > 0)
                PeakTime = $"{DateTime.Today.AddHours(peakHour.Hour):h tt} – {DateTime.Today.AddHours(peakHour.Hour + 1):h tt}";

            var peakDay = arrivals.GroupBy(t => t.EntryTime.DayOfWeek)
                .OrderByDescending(g => g.Count()).FirstOrDefault();
            if (peakDay != null) PeakDay = peakDay.Key.ToString();

            var topVehicle = arrivals.GroupBy(t => t.VehicleType)
                .OrderByDescending(g => g.Count()).FirstOrDefault();
            if (topVehicle != null)
            {
                MostParkedVehicleType = topVehicle.Key;
                MostParkedVehicleCount = topVehicle.Count();
            }

            for (var i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                WeeklySalesChart.Add(new DailySalesPoint
                {
                    Date = date,
                    Label = date.ToString("ddd"),
                    Amount = paid.Where(t => t.ExitTime!.Value.Date == date).Sum(t => t.TotalAmount)
                });
            }
        }
    }

    public class HourlyActivity
    {
        public int Hour { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DailySalesPoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
