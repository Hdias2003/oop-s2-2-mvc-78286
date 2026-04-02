using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafety.Web.Data
{
    public static class DataInitializer
    {
        // This method automatically fills the database with starting data if it's currently empty
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // SAFETY CHECK: If there are already businesses or users in the database, stop here.
            // We don't want to accidentally create double copies of everything.
            if (context.Premises.Any() || userManager.Users.Any()) return;

            // --- STEP 1: CREATE USER ROLES ---
            // We need "Admin" and "Inspector" categories to control what users can do.
            var adminRole = "Admin";
            var inspectorRole = "Inspector";

            if (!await roleManager.RoleExistsAsync(adminRole)) await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!await roleManager.RoleExistsAsync(inspectorRole)) await roleManager.CreateAsync(new IdentityRole(inspectorRole));

            // --- STEP 2: CREATE A TEST ADMIN ACCOUNT ---
            var adminEmail = "admin@council.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                // Create the user with a simple default password
                var result = await userManager.CreateAsync(user, "Password123!");
                // If the user was created successfully, give them the Admin role
                if (result.Succeeded) await userManager.AddToRoleAsync(user, adminRole);
            }

            // --- STEP 3: CREATE A TEST INSPECTOR ACCOUNT ---
            var inspectorEmail = "inspector@council.com";
            if (await userManager.FindByEmailAsync(inspectorEmail) == null)
            {
                var user = new IdentityUser { UserName = inspectorEmail, Email = inspectorEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(user, inspectorRole);
            }

            // --- STEP 4: CREATE 12 FAKE BUSINESSES (PREMISES) ---
            var towns = new[] { "Dublin", "Cork", "Galway" };
            var premisesList = new List<Premises>();

            for (int i = 1; i <= 12; i++)
            {
                premisesList.Add(new Premises
                {
                    Name = $"Premise {i} - Food Safety Ltd.",
                    Address = $"{i * 10} Food Avenue",
                    // Rotate through our list of 3 towns (Dublin, then Cork, then Galway, then repeat)
                    Town = towns[(i - 1) % towns.Length],
                    // Assign a mix of High, Medium, and Low risk levels
                    RiskRating = (RiskLevel)((i % 3) + 1)
                });
            }
            context.Premises.AddRange(premisesList); // Prepare to add the whole list
            await context.SaveChangesAsync(); // Push all 12 businesses into the database

            // --- STEP 5: CREATE 25 FAKE INSPECTION RECORDS ---
            var inspectionsList = new List<Inspection>();
            var random = new Random(); // Used to generate random scores and dates

            for (int i = 1; i <= 25; i++)
            {
                // Assign each inspection to one of our 12 businesses
                var associatedPremise = premisesList[(i - 1) % premisesList.Count];

                // Pick a random date from the last 100 days
                var inspectionDate = DateTime.Now.AddDays(-(random.Next(1, 100)));

                inspectionsList.Add(new Inspection
                {
                    PremisesId = associatedPremise.Id,
                    InspectionDate = inspectionDate,
                    Score = random.Next(50, 100),
                    // If the random number is high, they pass; otherwise, they fail
                    Outcome = (random.Next(50, 100) > 70) ? "Pass" : "Fail",
                    Notes = $"Routine inspection #{i} for {associatedPremise.Name}."
                });
            }
            context.Inspections.AddRange(inspectionsList);
            await context.SaveChangesAsync();

            // --- STEP 6: CREATE 10 FOLLOW-UP TASKS ---
            var followUpsList = new List<FollowUp>();

            // Only create follow-ups for the inspections that "Failed"
            var failedInspections = inspectionsList.Where(i => i.Outcome == "Fail").ToList();

            if (!failedInspections.Any()) return;

            for (int i = 0; i < 10; i++)
            {
                var associatedInspection = failedInspections[i % failedInspections.Count];

                // SCENARIO A: Make 4 tasks that are "OVERDUE" (Due date is in the past)
                if (i < 4)
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Open",
                        DueDate = DateTime.Now.AddDays(-(i + 1)), // Date is yesterday or older
                        ClosedDate = null
                    });
                }
                // SCENARIO B: Make 3 tasks that are "FINISHED" (Status is Closed)
                else if (i < 7)
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Closed",
                        DueDate = DateTime.Now.AddDays(-(i + 10)),
                        ClosedDate = DateTime.Now.AddDays(-(i + 5)) // Record when it was finished
                    });
                }
                // SCENARIO C: Make 3 tasks that are "UPCOMING" (Not due yet)
                else
                {
                    followUpsList.Add(new FollowUp
                    {
                        InspectionId = associatedInspection.Id,
                        Status = "Open",
                        DueDate = DateTime.Now.AddDays((i + 1)), // Due in the future
                        ClosedDate = null
                    });
                }
            }
            context.FollowUps.AddRange(followUpsList);
            await context.SaveChangesAsync();
        }
    }
}