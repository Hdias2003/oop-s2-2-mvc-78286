using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FoodSafety.Domain.Models
{
    // This class defines what a "FollowUp" task looks like in our database
    public class FollowUp : IValidatableObject
    {
        // [Key] tells the database this is the unique ID number for this record
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // [Required] means this field cannot be left blank
        [Required]
        public int InspectionId { get; set; }

        [Required]
        [DataType(DataType.Date)] // Tells the browser to show a date-picker calendar
        [Display(Name = "Due Date")] // Changes the label on the screen from "DueDate" to "Due Date"
        public DateTime DueDate { get; set; }

        [Required]
        // This ensures the user can only type "Open" or "Closed" (nothing else!)
        [RegularExpression("^(Open|Closed)$", ErrorMessage = "Status must be 'Open' or 'Closed'")]
        public string Status { get; set; } = "Open";

        [DataType(DataType.Date)]
        [Display(Name = "Date Closed")]
        // The '?' means this date is optional (it can be empty if the task is still open)
        public DateTime? ClosedDate { get; set; }

        // This links the FollowUp to a specific Inspection in the database
        [ValidateNever] // Tells the form not to worry about validating the whole Inspection object here
        [ForeignKey("InspectionId")]
        public virtual Inspection? Inspection { get; set; }

        // --- THE RULEBOOK (Custom Validation) ---
        // This code runs automatically to check for mistakes before saving
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Rule 1: If you say the task is "Closed," you must provide the date it was finished
            if (Status == "Closed" && !ClosedDate.HasValue)
            {
                yield return new ValidationResult(
                    "A Closed Date is required when the status is set to 'Closed'.",
                    new[] { nameof(ClosedDate) });
            }

            // Rule 2: If the task is still "Open," the "Date Closed" field should be empty
            if (Status == "Open" && ClosedDate.HasValue)
            {
                yield return new ValidationResult(
                    "Closed Date should be empty if the status is still 'Open'.",
                    new[] { nameof(ClosedDate) });
            }

            // Rule 3: You can't finish a task before its due date (logical check)
            if (Status == "Closed" && ClosedDate.HasValue && DueDate != default)
            {
                if (ClosedDate.Value < DueDate)
                {
                    yield return new ValidationResult(
                        $"The Closed Date ({ClosedDate.Value.ToShortDateString()}) cannot be earlier than the Due Date ({DueDate.ToShortDateString()}).",
                        new[] { nameof(ClosedDate) });
                }
            }
        }
    }
}