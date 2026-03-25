namespace oop_s2_2_mvc_78286.Models

{
    public class Inspection
    {
        public int Id { get; set; }
       
        public int PremisesId { get; set; } // Foreign Key 
        public DateTime InspectionDate { get; set; }
        
        public int Score { get; set; } // 0-100 
        public string Outcome { get; set; } // Pass/Fail 
        public string Notes { get; set; }
        

        // Navigation Properties
        public Premises Premises { get; set; }

        
        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}