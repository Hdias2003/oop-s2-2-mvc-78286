using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace oop_s2_2_mvc_78286.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Setup: Bring in the logger so we can record when things go wrong
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // The Home/Landing page of the website
        public IActionResult Index()
        {
            return View();
        }

        // The Privacy Policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // --- ERROR HANDLING ---
        // This method runs when something goes wrong (like a 404 or a code crash)
        [AllowAnonymous] // Everyone can see the error page, even if they aren't logged in
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Don't save/cache error pages
        public IActionResult Error(int? id)
        {
            // 1. Try to get the specific details of the error that just happened
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // Start with a generic "catch-all" message for the user
            string userFriendlyMessage = "An unexpected error occurred while processing your request.";

            // 2. Check if the error was a "404 - Not Found" (user typed a bad URL)
            if (id == 404)
            {
                userFriendlyMessage = "The page you are looking for does not exist (404).";

                // Log a warning so developers know people are hitting dead links
                _logger.LogWarning("404 Error: User tried to go to a path that doesn't exist: {Path}",
                    HttpContext.Request.Path);
            }
            // 3. If it wasn't a 404, check if there was a real code exception/crash
            else if (exceptionHandlerPathFeature != null)
            {
                // Write the full technical error details to our log file for debugging later
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "A crash occurred at {Path}. Error: {Message}",
                    exceptionHandlerPathFeature.Path, exceptionHandlerPathFeature.Error.Message);

                // Update the message to show the actual error text to the user
                userFriendlyMessage = exceptionHandlerPathFeature.Error.Message;
            }

            // 4. Send the error details to the View so they appear on the screen
            return View(new ErrorViewModel
            {
                // RequestId helps developers track this specific incident in the logs
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = userFriendlyMessage
            });
        }

        // This version of the Error method handles system-level crashes 
        // that might happen outside of a specific controller action
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // Log a "Critical" message because this is a serious system-level failure
            _logger.LogCritical("A major system error happened at {Path}. Message: {Msg}",
                feature?.Path, feature?.Error.Message);

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = feature?.Error.Message ?? "Unknown Error"
            });
        }
    }
}