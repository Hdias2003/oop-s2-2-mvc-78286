using FoodSafety.Domain.Models;
using FoodSafety.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class FollowUpQueryTests : TestBase
    {
        [Fact]
        public async Task OverdueQuery_FiltersCorrectItems()
        {
            // --- Arrange ---
            using var context = GetInMemoryContext();
            var today = DateTime.Today;

            // 1. Create Premises (The Root)
            var premise = new Premises { Name = "Test Shop", Town = "Dublin", RiskRating = RiskLevel.High };
            context.Premises.Add(premise);
            await context.SaveChangesAsync();

            // 2. Create Inspection (The Parent)
            var inspection = new Inspection
            {
                PremisesId = premise.Id,
                InspectionDate = today.AddDays(-10),
                Outcome = "Fail",
                
            };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            // 3. Create FollowUps linked to that Inspection ID
            context.FollowUps.AddRange(
                new FollowUp { InspectionId = inspection.Id, Status = "Open", DueDate = today.AddDays(-1) }, // Overdue
                new FollowUp { InspectionId = inspection.Id, Status = "Open", DueDate = today.AddDays(1) },  // Not due
                new FollowUp { InspectionId = inspection.Id, Status = "Closed", DueDate = today.AddDays(-5), ClosedDate = today } // Closed
            );
            await context.SaveChangesAsync();

            // --- Act ---
            var results = await context.FollowUps
                .Where(f => f.Status == "Open" && f.DueDate < today)
                .ToListAsync();

            // --- Assert ---
            Assert.Single(results);
            Assert.Equal("Open", results[0].Status);
            Assert.True(results[0].DueDate < today);
        }
    }
}