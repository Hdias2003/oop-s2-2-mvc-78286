using FoodSafety.Domain.Models; // Change to your actual namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafety.Web.Data
{
    public static class DataInitializer
    {
        // Change the context type to match your ApplicationDbContext
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // --- CRITICAL RULE: Don't duplicate data if it exists ---
            if (context.Premises.Any() || userManager.Users.Any()) return;

            // ==========================================================
            // -> 2 users, 1 as admin, 1 as inspector
            // ==========================================================
            var adminRole = "Admin";
            var inspectorRole = "Inspector";

            if (!await roleManager.RoleExistsAsync(adminRole)) await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!await roleManager.RoleExistsAsync(inspectorRole)) await roleManager.CreateAsync(new IdentityRole(inspectorRole));

            // Create ADMIN user
            var adminEmail = "admin@council.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                // Using the specific password
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(user, adminRole);
            }

            // Create INSPECTOR user
            var inspectorEmail = "inspector@council.com";
            if (await userManager.FindByEmailAsync(inspectorEmail) == null)
            {
                var user = new IdentityUser { UserName = inspectorEmail, Email = inspectorEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(user, inspectorRole);
            }

            // ==========================================================
            // -> 12 premises across 3 towns
            // ==========================================================
            var towns = new[] { "Dublin", "Cork", "Galway" };
            var premisesList = new List<Premises>();

            for (int i = 1; i <= 12; i++)
            {
                premisesList.Add(new Premises
                {
                    Name = $"Premise {i} - Food Safety Ltd.",
                    Address = $"{i * 10} Food Avenue",
                    // Spread across 3 towns
                    Town = towns[(i - 1) % towns.Length],
                    // Set a diverse RiskRating
                    RiskRating = (RiskLevel)((i % 3) + 1) // High, Med, Low
                });
            }
            context.Premises.AddRange(premisesList);
            await context.SaveChangesAsync();

            // ==========================================================
            // -> 25 inspections across different dates
            // ==========================================================
            var inspectionsList = new List<Inspection>();
            var random = new Random();

            for (int i = 1; i <= 25; i++)
            {
                // Ensure every premise is inspected, with some getting multiple visits
                var associatedPremise = premisesList[(i - 1) % premisesList.Count];

                // Inspections must have historical dates
                var inspectionDate = DateTime.Now.AddDays(-(random.Next(1, 100)));

                inspectionsList.Add(new Inspection
                {
                    PremisesId = associatedPremise.Id,
                    InspectionDate = inspectionDate,
                    Score = random.Next(50, 100),
                    // High scores pass, low scores fail
                    Outcome = (random.Next(50, 100) > 70) ? "Pass" : "Fail",
                    Notes = $"Routine inspection #{i} for {associatedPremise.Name}. Score was good but hygiene must improve."
                });
            }
            context.Inspections.AddRange(inspectionsList);
            await context.SaveChangesAsync();

            // ==========================================================
            // -> 10 follow-ups (some overdue, some closed)
            // ==========================================================
            var followUpsList = new List<FollowUp>();

            // Identify inspections that failed - these need follow-ups
            var failedInspections = inspectionsList.Where(i => i.Outcome == "Fail").ToList();

            if (!failedInspections.Any()) return; // Rare, but check for safety

            for (int i = 0; i < 10; i++)
            {
                // Ensure we link to unique failed inspections first
                var associatedInspection = failedInspections[i % failedInspections.Count];

                // --- Scenario 1: OVERDUE follow-ups ---
                if (i < 4) // 4 overdue items
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Open",
                        // Due date is in the past, triggering overdue logic
                        DueDate = DateTime.Now.AddDays(-(i + 1)),
                        ClosedDate = null
                    });
                }
                // --- Scenario 2: CLOSED follow-ups ---
                else if (i < 7) // 3 closed items
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Closed",
                        DueDate = DateTime.Now.AddDays(-(i + 10)),
                        // Set ClosedDate for audit
                        ClosedDate = DateTime.Now.AddDays(-(i + 5))
                    });
                }
                // --- Scenario 3: Upcoming follow-ups ---
                else // 3 open but not yet due
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Open",
                        DueDate = DateTime.Now.AddDays((i + 1)),
                        ClosedDate = null
                    });
                }
            }
            context.FollowUps.AddRange(followUpsList);
            await context.SaveChangesAsync();
        }
    }
}