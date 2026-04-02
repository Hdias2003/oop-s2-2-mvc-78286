using FoodSafety.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Security: By default, only Admins and Inspectors are allowed to add, change, or delete businesses
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        // Setup: Link this controller to the database and the activity logger
        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- LIST PAGE ---
        // Shows all registered businesses. We allow 'Viewers' to see this list as well.
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            // Pull the full list of businesses from the database and show them on the page
            return View(await _context.Premises.ToListAsync());
        }

        // --- DETAILS PAGE ---
        // Shows specific info for one business, including its past inspections
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Look up the business by ID. 
            // We use .Include to make sure we also grab the list of Inspections linked to it.
            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null) return NotFound();

            return View(premises);
        }

        // --- CREATE PAGE (GET) ---
        // Opens a blank form to register a new business location
        public IActionResult Create()
        {
            return View();
        }

        // --- CREATE (POST) ---
        // Saves the new business info to the database when the user clicks 'Submit'
        [HttpPost]
        [ValidateAntiForgeryToken] // Security feature to make sure the form is valid
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            // Check if the user filled out the required fields correctly
            if (ModelState.IsValid)
            {
                _context.Add(premises); // Add the new business to our list
                await _context.SaveChangesAsync(); // Save the changes to the database

                // Log a note so we have a record of who added this business and when
                _logger.LogInformation("New Premises created: {Name} in {Town} with Risk Level {Risk}",
                    premises.Name, premises.Town, premises.RiskRating);

                return RedirectToAction(nameof(Index)); // Go back to the main list
            }
            return View(premises);
        }

        // --- EDIT PAGE (GET) ---
        // Finds an existing business and opens it in a form so the user can update it
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _context.Premises.FindAsync(id);
            if (premises == null) return NotFound();

            return View(premises);
        }

        // --- EDIT (POST) ---
        // Saves the updated information for a business
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (id != premises.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(premises); // Apply the changes
                    await _context.SaveChangesAsync(); // Save to database

                    _logger.LogInformation("Premises updated: ID {Id}, Name {Name}", premises.Id, premises.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Error check: Make sure the business wasn't deleted by someone else while you were editing
                    if (!PremisesExists(premises.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        // --- DELETE PAGE (GET) ---
        // Shows a "Are you sure you want to delete this?" confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _context.Premises
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null) return NotFound();

            return View(premises);
        }

        // --- DELETE (POST) ---
        // Actually removes the business from the database after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises != null)
            {
                _context.Premises.Remove(premises); // Remove the record
                await _context.SaveChangesAsync(); // Save changes

                // Log a warning because deleting data is a major action
                _logger.LogWarning("Premises deleted: ID {Id}, Name {Name}", id, premises.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper Method: A quick way to check if a Business ID exists in our records
        private bool PremisesExists(int id)
        {
            return _context.Premises.Any(e => e.Id == id);
        }
    }
}