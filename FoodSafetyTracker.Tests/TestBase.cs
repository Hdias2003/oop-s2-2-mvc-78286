using FoodSafety.Domain.Models;
using oop_s2_2_mvc_78286.Data; 
using Microsoft.EntityFrameworkCore;

namespace FoodSafety.Tests.Base
{
    public abstract class TestBase
    {
        protected ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        }
    }
}