using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FoodSafety.Domain.Models
{
    public class FollowUp : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Required]
        [RegularExpression("^(Open|Closed)$", ErrorMessage = "Status must be 'Open' or 'Closed'")]
        public string Status { get; set; } = "Open";

        [DataType(DataType.Date)]
        [Display(Name = "Date Closed")]
        public DateTime? ClosedDate { get; set; }

        [ValidateNever]
        [ForeignKey("InspectionId")]
        
        public virtual Inspection? Inspection { get; set; }

        // Custom Validation Logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == "Closed" && !ClosedDate.HasValue)
            {
                yield return new ValidationResult(
                    "A Closed Date is required when the status is set to 'Closed'.",
                    new[] { nameof(ClosedDate) });
            }

            if (Status == "Open" && ClosedDate.HasValue)
            {
                yield return new ValidationResult(
                    "Closed Date should be empty if the status is still 'Open'.",
                    new[] { nameof(ClosedDate) });
            }
        }
    }
}