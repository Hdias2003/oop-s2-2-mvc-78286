using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels;

namespace oop_s2_2_mvc_78286.Data
{
    public static class DbInitializer
    {
        // Updated to Task for async support and accepts IServiceProvider to access Identity managers
        public static async Task Seed(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. SEED ROLES
            // Resolves the RoleManager from the service provider
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Defined roles as per assessment requirements: Admin, Inspector, Viewer
            string[] roleNames = { UserRoles.Admin, UserRoles.Inspector, UserRoles.Viewer };

            foreach (var roleName in roleNames)
            {
                // Checks if role exists, if not, creates it
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. SEED DEFAULT USERS
            // Resolves the UserManager from the service provider
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create a default Inspector user for testing
            var inspectorEmail = "inspector@council.com";
            if (await userManager.FindByEmailAsync(inspectorEmail) == null)
            {
                var user = new IdentityUser
                {
                    UserName = inspectorEmail,
                    Email = inspectorEmail,
                    EmailConfirmed = true
                };

                // Creates user with a default password
                await userManager.CreateAsync(user, "Password123!");
                // Assigns the Inspector role
                await userManager.AddToRoleAsync(user, UserRoles.Inspector);
            }

            // 3. SEED BUSINESS DATA
            // Only runs if the Premises table is empty to avoid duplicates
            if (context.Premises.Any()) return;

            // Seed 12 Premises as required by brief
            var premises = new List<Premises>
            {
                new Premises { Name = "The Salty Dog", Address = "12 Quay St", Town = "Dublin", RiskRating = RiskLevel.High },
                new Premises { Name = "Burger Heaven", Address = "5 Main St", Town = "Galway", RiskRating = RiskLevel.Medium },
                new Premises { Name = "Pasta Point", Address = "9 Cork Rd", Town = "Cork", RiskRating = RiskLevel.Low },
                // ... Add remaining to reach 12
            };
            context.Premises.AddRange(premises);
            context.SaveChanges();

            // Seed 25 Inspections as required by brief
            var inspections = new List<Inspection>
            {
                new Inspection { PremisesId = 1, InspectionDate = DateTime.Now.AddDays(-10), Score = 45, Outcome = "Fail", Notes = "Hygiene issues" },
                new Inspection { PremisesId = 2, InspectionDate = DateTime.Now.AddDays(-5), Score = 85, Outcome = "Pass", Notes = "All clear" },
                // ... Add remaining to reach 25
            };
            context.Inspections.AddRange(inspections);
            context.SaveChanges();

            // Seed 10 Follow-ups as required by brief (including overdue and closed)
            var followUps = new List<FollowUp>
            {
                // Overdue example: Status Open and DueDate in the past
                new FollowUp { InspectionId = 1, DueDate = DateTime.Now.AddDays(-2), Status = "Open" },
                // Closed example
                new FollowUp { InspectionId = 1, DueDate = DateTime.Now.AddDays(5), Status = "Closed", ClosedDate = DateTime.Now },
                // ... Add remaining to reach 10
            };
            context.FollowUps.AddRange(followUps);
            context.SaveChanges();
        }
    }
}