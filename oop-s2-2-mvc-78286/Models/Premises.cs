namespace oop_s2_2_mvc_78286.Models

{
    public enum RiskLevel { Low, Medium, High }

    public class Premises
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        public string Town { get; set; }
        
        public RiskLevel RiskRating { get; set; }
        

       
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}