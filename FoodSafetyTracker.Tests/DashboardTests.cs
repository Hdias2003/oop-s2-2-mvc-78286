using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodSafety.Domain.Models;
using oop_s2_2_mvc_78286.Data;
using System.Linq;

namespace FoodSafety.Tests
{
    public class DashboardTests
    {
        private ApplicationDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Dashboard_Counts_FailedInspectionsOnly()
        {
            // Arrange
            var context = GetInMemoryDb();

            // Adding all required fields: Notes, Score, Outcome, and Date
            context.Inspections.Add(new Inspection
            {
                Outcome = "Fail",
                Score = 45,
                Notes = "Required failed note", // Fixed: Was missing
                InspectionDate = DateTime.Now,
                PremisesId = 1
            });

            context.Inspections.Add(new Inspection
            {
                Outcome = "Pass",
                Score = 95,
                Notes = "Required passed note", // Fixed: Was missing
                InspectionDate = DateTime.Now,
                PremisesId = 1
            });

            await context.SaveChangesAsync();

            // Act
            var failedCount = await context.Inspections.CountAsync(i => i.Outcome == "Fail");

            // Assert
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task Dashboard_Filters_PremisesByTown()
        {
            // Arrange
            var context = GetInMemoryDb();

            context.Premises.Add(new Premises
            {
                Name = "Shop 1",
                Town = "Dublin",
                Address = "123 Dublin Road" // Fixed: Was missing
            });

            context.Premises.Add(new Premises
            {
                Name = "Shop 2",
                Town = "Galway",
                Address = "456 Galway Street" // Fixed: Was missing
            });

            await context.SaveChangesAsync();

            // Act
            var dublinList = await context.Premises.Where(p => p.Town == "Dublin").ToListAsync();

            // Assert
            Assert.Single(dublinList);
            Assert.Equal("Dublin", dublinList[0].Town);
        }
    }
}