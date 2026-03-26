using FoodSafety.Domain.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class FollowUpValidationTests
    {
        [Fact]
        public void ClosedStatus_Requires_ClosedDate()
        {
            var followUp = new FollowUp
            {
                Status = "Closed",
                DueDate = DateTime.Today,
                ClosedDate = null // Invalid state
            };

            var context = new ValidationContext(followUp);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(followUp, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage.Contains("Closed Date is required"));
        }
    }
}