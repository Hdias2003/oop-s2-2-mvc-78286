using System.Collections.Generic;
using FoodSafety.Domain.Models;

namespace FoodSafety.Domain.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary counts for the top of the page
        public int TotalInspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueFollowUpsCount { get; set; }

        // The list of overdue items to display in a table
        // INITIALIZATION: Setting this to a new List prevents NullReferenceExceptions in the View
        public List<FollowUp> OverdueFollowUps { get; set; } = new List<FollowUp>();

        // Filtering properties
        public string? SelectedTown { get; set; }

        // Ensure RiskLevel matches your Domain Enum
        public RiskLevel? SelectedRisk { get; set; }

        // Added for Serilog: To track which filters are being used in logs
        public string FilterSummary => $"Town: {SelectedTown ?? "All"}, Risk: {SelectedRisk?.ToString() ?? "All"}";
    }
}