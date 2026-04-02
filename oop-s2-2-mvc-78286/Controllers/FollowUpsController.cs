using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Security: Only Admins and Inspectors are allowed to use this controller by default
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        // Setup: Connect the controller to the database and the activity logger
        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- LIST PAGE ---
        // Shows all follow-up tasks. Viewers are also allowed to see this list.
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            // Get the list of follow-ups and include the inspection details for each
            var applicationDbContext = _context.FollowUps.Include(f => f.Inspection);
            return View(await applicationDbContext.ToListAsync());
        }

        // --- DETAILS PAGE ---
        // Shows info for one specific follow-up task based on its ID
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); // If no ID was provided, show 404 error

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound(); // If the ID doesn't exist in the DB, show 404

            return View(followUp);
        }

        // --- CREATE PAGE (GET) ---
        // Opens the blank form to create a new follow-up
        public IActionResult Create()
        {
            // Get all inspections so we can put them in a dropdown menu
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();

            // Format the text for the dropdown so it shows the ID, Business Name, and Date
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new
            {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText");

            return View();
        }

        // --- EDIT PAGE (GET) ---
        // Opens the form to edit an existing follow-up
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            // Prepare the dropdown menu for the edit screen
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new
            {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }

        // --- CREATE (POST) ---
        // Saves the new follow-up to the database after the user clicks 'Submit'
        [HttpPost]
        [ValidateAntiForgeryToken] // Security check to prevent form tampering
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            try
            {
                if (ModelState.IsValid) // Check if the user filled in the form correctly
                {
                    var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                    // Rule: You can't set a due date that happened BEFORE the inspection date
                    if (inspection != null && followUp.DueDate <= inspection.InspectionDate)
                    {
                        _logger.LogWarning("The user tried to set a due date before the inspection date.");
                        ModelState.AddModelError("DueDate", $"Due Date must be after the Inspection Date ({inspection.InspectionDate.ToShortDateString()})");
                    }
                    else
                    {
                        _context.Add(followUp); // Add to database
                        await _context.SaveChangesAsync(); // Save changes
                        return RedirectToAction(nameof(Index)); // Go back to the list
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while saving the new follow-up");
                return RedirectToAction("Error", "Home");
            }

            // If there was an error, reload the dropdown so the user can try again
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new
            {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }

        // --- EDIT (POST) ---
        // Saves changes to an existing follow-up
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                    // Re-check the date rule during editing
                    if (inspection != null && followUp.DueDate <= inspection.InspectionDate)
                    {
                        ModelState.AddModelError("DueDate", $"Due Date must be after the Inspection Date ({inspection.InspectionDate.ToShortDateString()})");
                    }
                    else
                    {
                        _context.Update(followUp); // Update the record
                        await _context.SaveChangesAsync(); // Save changes
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // This happens if two people try to edit the same record at the exact same time
                if (!FollowUpExists(followUp.Id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating follow-up {Id}", id);
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message
                });
            }

            // Reload dropdown if we had to return to the form due to a mistake
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new
            {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }


        // --- DELETE PAGE (GET) ---
        // Shows a confirmation page before deleting
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // --- DELETE (POST) ---
        // Actually removes the item from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
                _logger.LogWarning("A follow-up record was deleted.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper Method: A quick way to check if a specific Follow-up ID exists in the DB
        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}