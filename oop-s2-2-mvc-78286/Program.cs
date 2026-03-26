using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78286.Data;
using oop_s2_2_mvc_78286.Middleware; // Namespace for your custom middleware
using Serilog;
using Serilog.Events;

// 1. SERILOG BOOTSTRAP CONFIGURATION
// Configures the logger before the application fully starts to catch startup errors.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Filters out verbose framework logs
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodSafetyTracker") // Adds 'Application' name to every log
    .Enrich.WithEnvironmentName() // Adds 'Development' or 'Production' to every log
    .WriteTo.Console() // Outputs logs to the Visual Studio Debug console
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // Creates a new log file daily
    .CreateLogger();

try
{
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);

    // 2. INTEGRATE SERILOG WITH ASP.NET CORE
    // Replaces the default built-in logger with Serilog
    builder.Host.UseSerilog();

    // 3. DATABASE CONFIGURATION
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // 4. IDENTITY & ROLE CONFIGURATION
    // Configures user authentication and enables the Roles (Admin, Inspector, Viewer)
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // 5. GLOBAL EXCEPTION HANDLING (PIPELINE)
    // Ensures unhandled errors show a 'Friendly' page and are logged
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    // Authentication must run before we push UserName into the LogContext
    app.UseAuthentication();

    // Move the middleware here so it runs after authentication and before authorization
    app.UseMiddleware<UserLoggingMiddleware>();

    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();
    app.MapRazorPages()
       .WithStaticAssets();

    // 8. DATABASE SEEDING
    // Automatically populates the DB with the 12 premises and 25 inspections on startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        // FIX: Pass both the 'services' and 'context' arguments
        // Use .Wait() because the Seed method is likely defined as an 'async Task'
        DbInitializer.Seed(services, context).Wait();
    }

    app.Run();
}
catch (Exception ex)
{
    // Logs critical failures that prevent the app from starting
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    // Ensures all logs are written to the file before the process exits
    Log.CloseAndFlush();
}