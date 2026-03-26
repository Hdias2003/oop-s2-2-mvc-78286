using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels;
using oop_s2_2_mvc_78286.Data; // Ensure this matches your context namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace oop_s2_2_mvc_78286.Controllers
{
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        // Injected Logger for Serilog events
        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string town, string risk)
        {
            try
            {
                var today = DateTime.Today;
                var firstOfMonth = new DateTime(today.Year, today.Month, 1);

                // LOG 9: Audit - Tracking dashboard access and filter parameters
                _logger.LogInformation("Dashboard requested. Filters - Town: {Town}, Risk: {Risk}",
                    town ?? "None", risk ?? "None");

                // 1. Base query with essential Includes to prevent NullReference on Premises
                var overdueQuery = _context.FollowUps
                    .Include(f => f.Inspection)
                        .ThenInclude(i => i.Premises)
                    .Where(f => f.Status == "Open" && f.DueDate < today);

                // 2. Apply Filters
                if (!string.IsNullOrEmpty(town))
                {
                    overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town.Contains(town));
                }

                RiskLevel? selectedRiskEnum = null;
                if (Enum.TryParse<RiskLevel>(risk, out var riskEnum))
                {
                    overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == riskEnum);
                    selectedRiskEnum = riskEnum;
                }

                // 3. Execute query once
                var overdueList = await overdueQuery.ToListAsync();

                // LOG 10: Performance/Audit - Tracking how many items are being flagged
                if (overdueList.Count > 5)
                {
                    _logger.LogWarning("High volume of overdue action items detected: {Count} items.", overdueList.Count);
                }

                var viewModel = new DashboardViewModel
                {
                    TotalInspectionsThisMonth = await _context.Inspections
                        .CountAsync(i => i.InspectionDate >= firstOfMonth),

                    FailedInspectionsThisMonth = await _context.Inspections
                        .CountAsync(i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail"),

                    OverdueFollowUps = overdueList,
                    OverdueFollowUpsCount = overdueList.Count,
                    SelectedTown = town,
                    SelectedRisk = selectedRiskEnum
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // LOG 11: Error - Capture dashboard failures for Serilog
                _logger.LogError(ex, "Failed to load Dashboard data.");

                // Redirect to Home/Error with a specific message
                return RedirectToAction("Error", "Home");
            }
        }
    }
}