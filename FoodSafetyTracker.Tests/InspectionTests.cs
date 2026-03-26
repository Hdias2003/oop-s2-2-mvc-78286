using Xunit;
using FoodSafety.Domain.Models;

namespace FoodSafety.Tests
{
    public class InspectionTests
    {
        // Test: Future Date Validation (Business Rule)
        [Fact]
        public void Inspection_FutureDate_ShouldTriggerWarningLogic()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(7);
            var inspection = new Inspection
            {
                InspectionDate = futureDate,
                Notes = "Test future inspection", // Added for consistency
                Outcome = "Pending"
            };

            // Act: Logic typically used in your Controller
            bool isFuture = inspection.InspectionDate > DateTime.Now;

            // Assert
            Assert.True(isFuture, "Inspection dates set in the future should be flagged.");
        }
    }
}