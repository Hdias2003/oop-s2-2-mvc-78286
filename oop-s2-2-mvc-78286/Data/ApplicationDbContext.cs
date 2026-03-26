using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Premises> Premises { get; set; }
    public DbSet<Inspection> Inspections { get; set; }
    public DbSet<FollowUp> FollowUps { get; set; }
}