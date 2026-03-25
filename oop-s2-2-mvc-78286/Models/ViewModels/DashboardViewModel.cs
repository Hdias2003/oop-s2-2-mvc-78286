
namespace oop_s2_2_mvc_78286.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary counts for the top of the page
        public int TotalInspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueFollowUpsCount { get; set; }

        // The list of overdue items to display in a table
        public List<FollowUp> OverdueFollowUps { get; set; }

        // Filtering properties
        public string SelectedTown { get; set; }
        public RiskLevel? SelectedRisk { get; set; }
    }
}