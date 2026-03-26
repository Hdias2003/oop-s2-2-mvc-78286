using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using oop_s2_2_mvc_78286.Data;
using oop_s2_2_mvc_78286.Middleware;
using Serilog;
using Serilog.Events;

// Set a safe default MaxDepth to mitigate DoS while upgrading transitive usages.
// Add this before any JSON deserialize/serialize calls.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };

// 1. SERILOG BOOTSTRAP CONFIGURATION
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodSafetyTracker")
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);

    // 2. INTEGRATE SERILOG
    builder.Host.UseSerilog();

    // 3. DATABASE CONFIGURATION
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // 4. IDENTITY & ROLE CONFIGURATION
    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false; // Easier for testing your seeded users
        options.Password.RequiredLength = 6;
    })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // 5. GLOBAL EXCEPTION HANDLING (PIPELINE)
    if (app.Environment.IsDevelopment())
    {
        // Comment out or remove the Developer Page to test your friendly page
        // app.UseMigrationsEndPoint(); 

        // Force the use of the friendly error handler even in Development
        app.UseExceptionHandler("/Home/Error");
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Handles 404 Not Found by redirecting to a friendly page
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

    app.UseHttpsRedirection();
    app.UseStaticFiles(); // Replaces MapStaticAssets if using older Bootstrap versions
    app.UseRouting();

    app.UseAuthentication();

    // CUSTOM MIDDLEWARE: Log user actions
    app.UseMiddleware<UserLoggingMiddleware>();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    // 8. DATABASE SEEDING
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            // Ensures DB is deleted/recreated to apply your new Not-Null constraints
            // context.Database.EnsureDeleted(); // Uncomment to reset DB
            context.Database.Migrate();

            DbInitializer.Seed(services, context).Wait();
            Log.Information("Database Seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database seeding.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}