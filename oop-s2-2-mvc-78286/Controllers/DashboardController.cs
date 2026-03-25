using global::oop_s2_2_mvc_78286.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78286.Data;


namespace oop_s2_2_mvc_78286.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string town, string risk)
        {
            var today = DateTime.Today;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);

            // 1. Base query for follow-ups (Criteria for Overdue: Open AND DueDate < Today)
            var overdueQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .Where(f => f.Status == "Open" && f.DueDate < today);

            // 2. Apply Filters (Town and/or RiskRating)
            if (!string.IsNullOrEmpty(town))
            {
                overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town == town);
            }

            if (Enum.TryParse<Models.RiskLevel>(risk, out var riskEnum))
            {
                overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == riskEnum);
            }

            // 3. Build the ViewModel with Aggregations
            var viewModel = new DashboardViewModel
            {
                // Count inspections for the current month
                TotalInspectionsThisMonth = await _context.Inspections
                    .CountAsync(i => i.InspectionDate >= firstOfMonth),

                // Count failed inspections for the current month
                FailedInspectionsThisMonth = await _context.Inspections
                    .CountAsync(i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail"),

                // Get the filtered list and its count
                OverdueFollowUps = await overdueQuery.ToListAsync(),
                OverdueFollowUpsCount = await overdueQuery.CountAsync(),

                SelectedTown = town,
                SelectedRisk = riskEnum
            };

            return View(viewModel);
        }
    }
}