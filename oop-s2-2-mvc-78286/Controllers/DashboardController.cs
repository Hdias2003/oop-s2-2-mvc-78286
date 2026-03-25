using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78286.Data;
using oop_s2_2_mvc_78286.Models;
using oop_s2_2_mvc_78286.Models.ViewModels;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Restricts access to all roles defined in the assessment
    // This ensures only authorized personnel can see the aggregated stats
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // The Index action handles the logic for the summary cards and the filtered overdue table
        public async Task<IActionResult> Index(string town, string risk)
        {
            // Time variables for "This Month" logic
            var today = DateTime.Today;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);

            // 1. Base query for follow-ups
            // Includes navigational properties to access Premises data from the FollowUp entity
            var overdueQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .Where(f => f.Status == "Open" && f.DueDate < today);

            // 2. Apply Filters based on User Input (Town and/or RiskRating)
            if (!string.IsNullOrEmpty(town))
            {
                overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town == town);
            }

            // Attempts to parse the risk string into the RiskLevel Enum
            if (Enum.TryParse<RiskLevel>(risk, out var riskEnum))
            {
                overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == riskEnum);
            }

            // 3. Build the ViewModel with Aggregations for the View
            var viewModel = new DashboardViewModel
            {
                // Aggregation: Count all inspections conducted since the 1st of the current month
                TotalInspectionsThisMonth = await _context.Inspections
                    .CountAsync(i => i.InspectionDate >= firstOfMonth),

                // Aggregation: Count only failed inspections for the current month
                FailedInspectionsThisMonth = await _context.Inspections
                    .CountAsync(i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail"),

                // List: Returns the specific follow-up records that match the 'Overdue' criteria and filters
                OverdueFollowUps = await overdueQuery.ToListAsync(),

                // Aggregation: The count of the filtered overdue list
                OverdueFollowUpsCount = await overdueQuery.CountAsync(),

                SelectedTown = town,
                SelectedRisk = riskEnum
            };

            return View(viewModel);
        }
    }
}