using FoodSafety.Domain.Models;
using FoodSafety.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class DashboardStatsTests : TestBase
    {
        [Fact]
        public async Task Dashboard_FailedCount_IsCorrect()
        {
            using var context = GetInMemoryContext();
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // 1. Create a parent Premises first to satisfy the Foreign Key constraint
            var testPremise = new Premises
            {
                Name = "Test Shop",
                Address = "123 Test St",
                Town = "Dublin",
                RiskRating = RiskLevel.Low
            };
            context.Premises.Add(testPremise);
            await context.SaveChangesAsync(); // Save now so testPremise gets an Id

            // 2. Add inspections linked to that Premise ID
            context.Inspections.AddRange(
                new Inspection
                {
                    InspectionDate = startOfMonth,
                    Outcome = "Fail",
                    
                    PremisesId = testPremise.Id // <--- Link the ID here
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

            // 3. This will now succeed!
            await context.SaveChangesAsync();

            var failedCount = await context.Inspections
                .CountAsync(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail");

            Assert.Equal(2, failedCount);
        }
    }
    }
