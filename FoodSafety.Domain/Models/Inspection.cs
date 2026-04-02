using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FoodSafety.Domain.Models
{
    // This class defines what an "Inspection" record looks like in the database
    public class Inspection
    {
        // [Key] tells the database this is the unique ID number for this specific inspection
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // This links the inspection to a specific Business (Premises) using its ID
        [Required]
        [ForeignKey("Premises")]
        public int PremisesId { get; set; }

        [Required]
        [DataType(DataType.Date)] // Tells the web browser to show a calendar date-picker
        public DateTime InspectionDate { get; set; }

        [Required]
        // This ensures the score is a sensible number (0 to 100)
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public int Score { get; set; }

        [Required]
        // This limits the user to only two choices: "Pass" or "Fail"
        [RegularExpression("^(Pass|Fail)$", ErrorMessage = "Outcome must be either 'Pass' or 'Fail'")]
        public string Outcome { get; set; } = string.Empty;

        // "string?" means notes are optional—the user can leave this blank
        [Display(Name = "Inspection Notes")]
        public string? Notes { get; set; }

        // --- CONNECTIONS (Navigation Properties) ---

        // This allows the code to easily "jump" from an Inspection to see the Business details
        [ValidateNever] // Prevents the form from checking the Business data when saving an Inspection
        public virtual Premises Premises { get; set; } = null!;

        // This creates a list (collection) of all Follow-up tasks linked to this specific inspection
        [ValidateNever]
        public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}