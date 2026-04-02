using FoodSafety.Domain.Models;
using FoodSafety.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class DashboardStatsTests : TestBase
    {
        // [Fact] tells the testing tool that this is a specific test case to run
        [Fact]
        public async Task Dashboard_FailedCount_IsCorrect()
        {
            // Create a temporary "fake" database in the computer's memory for this test
            using var context = GetInMemoryContext();

            // Get the date for the very first day of this month
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // --- STEP 1: SETUP (The "Arrange" Phase) ---
            // Create a test business first. 
            // We need this because an inspection can't exist without a business to belong to.
            var testPremise = new Premises
            {
                Name = "Test Shop",
                Address = "123 Test St",
                Town = "Dublin",
                RiskRating = RiskLevel.Low
            };
            context.Premises.Add(testPremise);

            // Save the business so the database generates a unique ID for it
            await context.SaveChangesAsync();

            // --- STEP 2: ADD TEST DATA ---
            // We are adding 3 inspections to our fake database: 2 Fails and 1 Pass.
            context.Inspections.AddRange(
                new Inspection
                {
                    InspectionDate = startOfMonth,
                    Outcome = "Fail",
                    // Link this inspection to the ID of the business we just created
                    PremisesId = testPremise.Id
                },
                new Inspection
                {
                    InspectionDate = startOfMonth,
                    Outcome = "Fail",
                    PremisesId = testPremise.Id
                },
                new Inspection
                {
                    InspectionDate = startOfMonth,
                    Outcome = "Pass",
                    PremisesId = testPremise.Id
                }
            );

            // Save these inspections into our fake database
            await context.SaveChangesAsync();

            // --- STEP 3: THE ACTION (The "Act" Phase) ---
            // Ask the database to count how many "Fail" results happened this month
            var failedCount = await context.Inspections
                .CountAsync(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail");

            // --- STEP 4: THE CHECK (The "Assert" Phase) ---
            // We verify if our logic is working. 
            // Since we added 2 failed inspections, the 'failedCount' should be exactly 2.
            Assert.Equal(2, failedCount);
        }
    }
}