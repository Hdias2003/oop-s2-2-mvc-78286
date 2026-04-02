using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels;
using oop_s2_2_mvc_78286.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Restrict access: Only users with Admin, Inspector, or Viewer roles can see this
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
    public class DashboardController : Controller
    {
        // These variables hold our database connection and our activity logger
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        // Constructor: This sets up the controller with the database and logger when it's created
        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // The main "Index" page for the Dashboard. It accepts optional filters for Town and Risk.
        public async Task<IActionResult> Index(string town, string risk)
        {
            try
            {
                // Get today's date and the date for the start of the current month
                var today = DateTime.Today;
                var firstOfMonth = new DateTime(today.Year, today.Month, 1);

                // Write a note to the log file showing what filters the user is searching for
                _logger.LogInformation("Dashboard requested. Filters - Town: {Town}, Risk: {Risk}",
                    town ?? "None", risk ?? "None");

                // 1. Prepare a search: Find "Open" follow-ups that are past their due date
                // We use .Include to make sure we also load the related Inspection and Premises data
                var overdueQuery = _context.FollowUps
                    .Include(f => f.Inspection)
                        .ThenInclude(i => i.Premises)
                    .Where(f => f.Status == "Open" && f.DueDate < today);

                // 2. Filter by Town: If the user typed a town name, add that to our search
                if (!string.IsNullOrEmpty(town))
                {
                    overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town.Contains(town));
                }

                // 2b. Filter by Risk: If the user chose a risk level, try to match it with our RiskLevel list
                RiskLevel? selectedRiskEnum = null;
                if (Enum.TryParse<RiskLevel>(risk, out var riskEnum))
                {
                    overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == riskEnum);
                    selectedRiskEnum = riskEnum;
                }

                // 3. Actually run the search against the database and put the results in a list
                var overdueList = await overdueQuery.ToListAsync();

                // If there are more than 5 overdue items, log a warning so staff are aware of the backlog
                if (overdueList.Count > 5)
                {
                    _logger.LogWarning("High volume of overdue action items detected: {Count} items.", overdueList.Count);
                }

                // 4. Create a "ViewModel" to send all this data to the web page (View)
                var viewModel = new DashboardViewModel
                {
                    // Count how many inspections happened this month
                    TotalInspectionsThisMonth = await _context.Inspections
                        .CountAsync(i => i.InspectionDate >= firstOfMonth),

                    // Count how many failed inspections happened this month
                    FailedInspectionsThisMonth = await _context.Inspections
                        .CountAsync(i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail"),

                    // Pass the list of overdue items and the filter settings to the page
                    OverdueFollowUps = overdueList,
                    OverdueFollowUpsCount = overdueList.Count,
                    SelectedTown = town,
                    SelectedRisk = selectedRiskEnum
                };

                // Send the data to the browser to be displayed
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // If something crashes, log the specific error details for the developers
                _logger.LogError(ex, "Failed to load Dashboard data.");

                // Send the user to a friendly error page instead of showing them code errors
                return RedirectToAction("Error", "Home");
            }
        }
    }
}