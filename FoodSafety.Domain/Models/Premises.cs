using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSafety.Domain.Models
{
    // The Enum acts like a fixed "multiple-choice" list
    // This ensures the user can only choose Low, Medium, or High
    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    // This class represents a "Premises" (a business or restaurant) in our system
    public class Premises
    {
        // [Key] tells the database this is the unique ID number for this business
        [Key]
        // This ensures the computer automatically gives each new business a unique number (1, 2, 3...)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // [Required] means the user is not allowed to leave the Name blank
        [Required]
        // This limits the name to 100 characters so it fits nicely on the screen and in the database
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Town { get; set; } = string.Empty;

        [Required]
        // This forces the user to pick one of the options from our RiskLevel list above
        public RiskLevel RiskRating { get; set; }

        // --- CONNECTIONS ---
        // This creates a "One-to-Many" relationship. 
        // It means one business can have a whole list (Collection) of many different inspections.
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}