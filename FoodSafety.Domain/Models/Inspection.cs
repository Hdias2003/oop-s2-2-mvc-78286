using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Required for ValidateNever


namespace FoodSafety.Domain.Models
{
    public class Inspection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Premises")]
        public int PremisesId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public int Score { get; set; }

        [Required]
        [RegularExpression("^(Pass|Fail)$", ErrorMessage = "Outcome must be either 'Pass' or 'Fail'")]
        public string Outcome { get; set; } = string.Empty;

        [Display(Name = "Inspection Notes")]
        public string? Notes { get; set; }

        // Navigation Properties
        [ValidateNever] // This prevents the 'refresh without saving' bug
        public virtual Premises Premises { get; set; } = null!;

        [ValidateNever] // Also add it here to be safe
        public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}