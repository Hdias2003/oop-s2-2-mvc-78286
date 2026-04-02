using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafety.Web.Data
{
    public static class DataInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // SAFETY CHECK: Stop if data already exists
            if (context.Premises.Any() || userManager.Users.Any()) return;

            // --- STEP 1: CREATE USER ROLES ---
            // Using your standardized UserRoles class constants
            string[] roles = { UserRoles.Admin, UserRoles.Inspector, UserRoles.Viewer };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // --- STEP 2: CREATE TEST ACCOUNTS ---

            // Admin
            await CreateUserHelper(userManager, "admin@council.com", "Password123!", UserRoles.Admin);

            // Inspector
            await CreateUserHelper(userManager, "inspector@council.com", "Password123!", UserRoles.Inspector);

            // Viewer (formerly 'user')
            await CreateUserHelper(userManager, "viewer@council.com", "Password123!", UserRoles.Viewer);

            // --- STEP 3: CREATE 12 FAKE BUSINESSES ---
            var towns = new[] { "Dublin", "Cork", "Galway" };
            var premisesList = new List<Premises>();

            for (int i = 1; i <= 12; i++)
            {
                premisesList.Add(new Premises
                {
                    Name = $"Premise {i} - Food Safety Ltd.",
                    Address = $"{i * 10} Food Avenue",
                    Town = towns[(i - 1) % towns.Length],
                    RiskRating = (RiskLevel)((i % 3) + 1)
                });
            }
            context.Premises.AddRange(premisesList);
            await context.SaveChangesAsync();

            // --- STEP 4: CREATE 25 FAKE INSPECTIONS ---
            var inspectionsList = new List<Inspection>();
            var random = new Random();

            for (int i = 1; i <= 25; i++)
            {
                var associatedPremise = premisesList[(i - 1) % premisesList.Count];
                var inspectionDate = DateTime.Now.AddDays(-(random.Next(1, 100)));

                inspectionsList.Add(new Inspection
                {
                    PremisesId = associatedPremise.Id,
                    InspectionDate = inspectionDate,
                    Score = random.Next(50, 100),
                    Outcome = (random.Next(50, 100) > 70) ? "Pass" : "Fail",
                    Notes = $"Routine inspection #{i} for {associatedPremise.Name}."
                });
            }
            context.Inspections.AddRange(inspectionsList);
            await context.SaveChangesAsync();

            // --- STEP 5: CREATE 10 FOLLOW-UP TASKS ---
            var followUpsList = new List<FollowUp>();
            var failedInspections = inspectionsList.Where(i => i.Outcome == "Fail").ToList();

            if (failedInspections.Any())
            {
                for (int i = 0; i < 10; i++)
                {
                    var associatedInspection = failedInspections[i % failedInspections.Count];

                    var status = i < 7 ? (i < 4 ? "Open" : "Closed") : "Open";
                    var dueDateOffset = i < 7 ? (i < 4 ? -(i + 1) : -(i + 10)) : (i + 1);

                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = status,
                        DueDate = DateTime.Now.AddDays(dueDateOffset),
                        ClosedDate = status == "Closed" ? DateTime.Now.AddDays(-(i + 5)) : null
                    });
                }
                context.FollowUps.AddRange(followUpsList);
                await context.SaveChangesAsync();
            }
        }

        // Helper method to reduce code repetition for user creation
        private static async Task CreateUserHelper(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}