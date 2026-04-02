using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Security: Only Admins and Inspectors can use these pages by default
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class InspectionsController : Controller
    {
        private readonly ILogger<InspectionsController> _logger;
        private readonly ApplicationDbContext _context;

        // Setup: Link the controller to our database and the activity logger
        public InspectionsController(ILogger<InspectionsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // --- LIST PAGE ---
        // Shows all inspections. We allow 'Viewers' to see this list too.
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            // Get the list of inspections and make sure to include the Business (Premises) name
            var applicationDbContext = _context.Inspections.Include(i => i.Premises);
            return View(await applicationDbContext.ToListAsync());
        }

        // --- DETAILS PAGE ---
        // Shows the history of all inspections for one specific business
        // Inside InspectionsController.cs
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // The .Include is the "Fix" here!
            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();

            return View(inspection);
        }
        // --- CREATE PAGE (GET) ---
        // Opens the form to log a new inspection
        public IActionResult Create()
        {
            // Create a dropdown list so the user can pick which business was inspected
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name");
            return View();
        }

        // --- CREATE (POST) ---
        // Saves the new inspection details when the user clicks 'Save'
        [HttpPost]
        [ValidateAntiForgeryToken] // Security check to ensure the form is legitimate
        public async Task<IActionResult> Create([Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            try
            {
                // Check if all required fields were filled out correctly
                if (ModelState.IsValid)
                {
                    // Logic check: Log a warning if the date entered is in the future
                    if (inspection.InspectionDate > DateTime.Now)
                    {
                        _logger.LogWarning("An inspection was recorded with a future date.");
                    }

                    _context.Add(inspection); // Add the new record
                    await _context.SaveChangesAsync(); // Save to the database

                    _logger.LogInformation("New inspection record created successfully.");

                    return RedirectToAction(nameof(Index)); // Go back to the list
                }
            }
            catch (Exception ex)
            {
                // If there's a database error, log the details and show the Error page
                _logger.LogError(ex, "Error occurred while creating inspection.");
                return View("Error");
            }

            // If the form had errors, reload the business dropdown so the user can fix it
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        // --- EDIT PAGE (GET) ---
        // Loads the data for an existing inspection so the user can change it
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return NotFound();

            // Prepare the dropdown list with the current business selected
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        // --- EDIT (POST) ---
        // Updates the database with the new information provided by the user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection); // Apply the changes
                    await _context.SaveChangesAsync(); // Save to database

                    _logger.LogInformation("Inspection ID {Id} was updated.", inspection.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Happens if someone else deleted or changed the record while you were editing it
                    if (!InspectionExists(inspection.Id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating inspection {Id}", inspection.Id);
                    return View("Error");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        // --- DELETE PAGE (GET) ---
        // Asks the user "Are you sure?" before deleting
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();

            return View(inspection);
        }

        // --- DELETE (POST) ---
        // Actually removes the inspection from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection ID {Id} was deleted.", id);
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper: Checks the database to see if a specific inspection ID still exists
        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}