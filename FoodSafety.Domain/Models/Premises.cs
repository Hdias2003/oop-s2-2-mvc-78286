using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSafety.Domain.Models
{
    // The Enum restricts RiskRating to only these 3 specific options
    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    public class Premises
    {
        [Key] // Sets ID as the Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensures it is unique and auto-incremented
        public int Id { get; set; }

        [Required] // Changes to NOT NULL in the database
        [StringLength(100)] // Improves efficiency by not using 'nvarchar(max)'
        public string Name { get; set; } = string.Empty;

        [Required] // Changes to NOT NULL
        public string Address { get; set; } = string.Empty;

        [Required] // Changes to NOT NULL
        public string Town { get; set; } = string.Empty;

        [Required] // Ensures a risk rating must be selected
        public RiskLevel RiskRating { get; set; }

        // Navigation property for the 1-to-Many relationship
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}