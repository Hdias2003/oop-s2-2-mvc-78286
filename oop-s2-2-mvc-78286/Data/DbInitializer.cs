using oop_s2_2_mvc_78286.Models;

namespace oop_s2_2_mvc_78286.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Premises.Any()) return; // Database has been seeded

            
            var premises = new List<Premises>
            {
                new Premises { Name = "The Salty Dog", Address = "12 Quay St", Town = "Dublin", RiskRating = RiskLevel.High },
                new Premises { Name = "Burger Heaven", Address = "5 Main St", Town = "Galway", RiskRating = RiskLevel.Medium },
                // ... add 10 more to reach 12
            };
            context.Premises.AddRange(premises);
            context.SaveChanges();

            
            var inspections = new List<Inspection>
            {
                new Inspection { PremisesId = 1, InspectionDate = DateTime.Now.AddDays(-10), Score = 45, Outcome = "Fail", Notes = "Hygiene issues" },
                new Inspection { PremisesId = 2, InspectionDate = DateTime.Now.AddDays(-5), Score = 85, Outcome = "Pass", Notes = "All clear" },
                // ... add 23 more across different dates
            };
            context.Inspections.AddRange(inspections);
            context.SaveChanges();

            
            var followUps = new List<FollowUp>
            {
                
                new FollowUp { InspectionId = 1, DueDate = DateTime.Now.AddDays(-2), Status = "Open" },
                // Closed example
                new FollowUp { InspectionId = 1, DueDate = DateTime.Now.AddDays(5), Status = "Closed", ClosedDate = DateTime.Now },
                // ... add 8 more
            };
            context.FollowUps.AddRange(followUps);
            context.SaveChanges();
        }
    }
}