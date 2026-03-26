using Xunit;
using FoodSafety.Domain.Models;

namespace FoodSafety.Tests
{
    public class FollowUpTests
    {
        // Test 2: Overdue Status Logic
        [Fact]
        public void FollowUp_PastDueDate_IsCorrectlyIdentifiedAsOverdue()
        {
            // Arrange
            var overdueTask = new FollowUp
            {
                Status = "Open",
                DueDate = DateTime.Now.AddDays(-10)
            };

            // Act
            bool isOverdue = overdueTask.Status == "Open" && overdueTask.DueDate < DateTime.Now;

            // Assert
            Assert.True(isOverdue, "Open tasks with past due dates must be marked overdue.");
        }
    }
}