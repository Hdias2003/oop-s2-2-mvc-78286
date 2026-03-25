namespace oop_s2_2_mvc_78286.Models

{
    public class FollowUp
    {
        public int Id { get; set; }
       
        public int InspectionId { get; set; } // Foreign Key 
        public DateTime DueDate { get; set; }
        
        public string Status { get; set; } // Open/Closed 
        public DateTime? ClosedDate { get; set; } // Nullable 

        // Navigation Property
        public Inspection Inspection { get; set; }
    }
}