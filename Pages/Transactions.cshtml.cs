using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using parkEASE.Data;
using parkEASE.Models;

namespace parkEASE.Pages
{
    [Authorize(Roles = "Admin")]
    public class TransactionsModel : PageModel
    {
        private readonly ParkEaseDbContext _context;
        public TransactionsModel(ParkEaseDbContext context) => _context = context;

        public List<Transaction> Transactions { get; set; } = new();
        public List<Transaction> ReportTransactions { get; set; } = new();
        public List<ReportBreakdown> VehicleBreakdown { get; set; } = new();
        public List<ReportBreakdown> PaymentBreakdown { get; set; } = new();
        public List<ReportDailySales> DailySales { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public string? PaymentFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ReportStartDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ReportEndDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal ReportRevenue { get; set; }
        public int ReportTotalRecords { get; set; }
        public int ReportPaidCount { get; set; }
        public int ReportParkedCount { get; set; }
        public double ReportAverageStayHours { get; set; }

        public async Task OnGetAsync()
        {
            var start = (ReportStartDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (ReportEndDate ?? DateTime.Today).Date;
            if (end < start) end = start;
            ReportStartDate = start;
            ReportEndDate = end;
            var endExclusive = end.AddDays(1);

            var allTransactions = await _context.Transactions
                .Include(t => t.ParkingSlot)
                .Include(t => t.User)
                .AsNoTracking()
                .ToListAsync();

            var query = allTransactions.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();
                query = query.Where(t => t.PlateNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                         t.TransactionID.Contains(term, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(PaymentFilter) && PaymentFilter != "All")
                query = query.Where(t => t.PaymentMethod == PaymentFilter);
            if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
                query = query.Where(t => t.PaymentStatus == StatusFilter);

            Transactions = query.OrderByDescending(t => t.CreatedAt).ToList();
            TotalTransactions = Transactions.Count;
            TotalRevenue = Transactions.Where(t => t.PaymentStatus == "Paid").Sum(t => t.TotalAmount);

            ReportTransactions = allTransactions
                .Where(t => t.EntryTime >= start && t.EntryTime < endExclusive)
                .OrderByDescending(t => t.EntryTime)
                .ToList();

            var reportPaid = allTransactions.Where(t => t.PaymentStatus == "Paid" && t.ExitTime.HasValue &&
                t.ExitTime.Value >= start && t.ExitTime.Value < endExclusive).ToList();
            ReportRevenue = reportPaid.Sum(t => t.TotalAmount);
            ReportTotalRecords = ReportTransactions.Count;
            ReportPaidCount = ReportTransactions.Count(t => t.PaymentStatus == "Paid");
            ReportParkedCount = ReportTransactions.Count(t => t.PaymentStatus == "Parked");

            var completedInReport = ReportTransactions.Where(t => t.ExitTime.HasValue && t.ExitTime.Value >= t.EntryTime).ToList();
            ReportAverageStayHours = completedInReport.Count == 0
                ? 0
                : completedInReport.Average(t => (t.ExitTime!.Value - t.EntryTime).TotalHours);

            VehicleBreakdown = reportPaid
                .GroupBy(t => t.VehicleType)
                .Select(g => new ReportBreakdown { Label = g.Key, Count = g.Count(), Amount = g.Sum(t => t.TotalAmount) })
                .OrderByDescending(x => x.Count).ToList();

            PaymentBreakdown = reportPaid
                .GroupBy(t => string.IsNullOrWhiteSpace(t.PaymentMethod) ? "Unspecified" : t.PaymentMethod)
                .Select(g => new ReportBreakdown { Label = g.Key, Count = g.Count(), Amount = g.Sum(t => t.TotalAmount) })
                .OrderByDescending(x => x.Amount).ToList();

            // Keep the chart readable for long reporting periods by grouping daily revenue into up to 31 buckets.
            var spanDays = (end - start).Days + 1;
            var bucketSize = Math.Max(1, (int)Math.Ceiling(spanDays / 31.0));
            for (var offset = 0; offset < spanDays; offset += bucketSize)
            {
                var bucketStart = start.AddDays(offset);
                var bucketEnd = bucketStart.AddDays(bucketSize);
                if (bucketEnd > endExclusive) bucketEnd = endExclusive;
                DailySales.Add(new ReportDailySales
                {
                    Label = bucketSize == 1 ? bucketStart.ToString("MMM d") : $"{bucketStart:MMM d}–{bucketEnd.AddDays(-1):MMM d}",
                    Amount = reportPaid.Where(t => t.ExitTime!.Value >= bucketStart && t.ExitTime.Value < bucketEnd).Sum(t => t.TotalAmount)
                });
            }
        }
    }

    public class ReportBreakdown
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class ReportDailySales
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
