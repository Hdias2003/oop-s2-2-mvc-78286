using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels; // Ensure this points to your actual ViewModel namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace oop_s2_2_mvc_78286.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            // 1. Capture the exception details from the platform
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // Default generic message
            string userFriendlyMessage = "An unexpected error occurred while processing your request.";

            // 2. Determine if this was a specific Status Code error (like 404)
            if (id == 404)
            {
                userFriendlyMessage = "The page you are looking for does not exist (404).";
                _logger.LogWarning("404 Error: User attempted to access non-existent path: {Path}",
                    HttpContext.Request.Path);
            }
            else if (exceptionHandlerPathFeature != null)
            {
                // 3. Meaningful Logging: Log the actual exception with Serilog
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "Unhandled exception occurred at {Path}. Error: {Message}",
                    exceptionHandlerPathFeature.Path, exceptionHandlerPathFeature.Error.Message);

                // Use the actual exception message for the user to see in the alert box
                userFriendlyMessage = exceptionHandlerPathFeature.Error.Message;
            }

            // 4. Return the View with the populated Model
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = userFriendlyMessage // This property must exist in your ErrorViewModel
            });
        }
    }
}