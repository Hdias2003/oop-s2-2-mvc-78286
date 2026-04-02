using FoodSafety.Domain.Models;
using FoodSafety.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class FollowUpQueryTests : TestBase
    {
        // This test checks if our search logic correctly finds "Overdue" items
        [Fact]
        public async Task OverdueQuery_FiltersCorrectItems()
        {
            // --- STEP 1: SETUP (The "Arrange" Phase) ---
            // Create a temporary "fake" database in memory for this test
            using var context = GetInMemoryContext();
            var today = DateTime.Today;

            // 1. Create the Business (The Root)
            // Databases require a parent business to exist before we can add inspections to it
            var premise = new Premises { Name = "Test Shop", Town = "Dublin", RiskRating = RiskLevel.High };
            context.Premises.Add(premise);
            await context.SaveChangesAsync();

            // 2. Create the Inspection (The Parent)
            // Now we link an inspection to the Business ID we just saved
            var inspection = new Inspection
            {
                PremisesId = premise.Id,
                InspectionDate = today.AddDays(-10), // Happened 10 days ago
                Outcome = "Fail",
            };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            // 3. Create three different Follow-up scenarios to test our filters
            context.FollowUps.AddRange(
                // Scenario A: Open and past the due date (This is the only one that is OVERDUE)
                new FollowUp { InspectionId = inspection.Id, Status = "Open", DueDate = today.AddDays(-1) },

                // Scenario B: Open but the due date is in the future (Not overdue yet)
                new FollowUp { InspectionId = inspection.Id, Status = "Open", DueDate = today.AddDays(1) },

                // Scenario C: Already finished (Status is Closed, so it shouldn't show up as overdue)
                new FollowUp { InspectionId = inspection.Id, Status = "Closed", DueDate = today.AddDays(-5), ClosedDate = today }
            );
            await context.SaveChangesAsync();

            // --- STEP 2: THE ACTION (The "Act" Phase) ---
            // We run a search query: "Find me items that are still 'Open' AND have a 'DueDate' before today"
            var results = await context.FollowUps
                .Where(f => f.Status == "Open" && f.DueDate < today)
                .ToListAsync();

            // --- STEP 3: THE CHECK (The "Assert" Phase) ---

            // We verify that the search found exactly 1 item (Scenario A)
            Assert.Single(results);

            // Double-check that the item found is actually 'Open'
            Assert.Equal("Open", results[0].Status);

            // Double-check that its due date is actually in the past
            Assert.True(results[0].DueDate < today);
        }
    }
}