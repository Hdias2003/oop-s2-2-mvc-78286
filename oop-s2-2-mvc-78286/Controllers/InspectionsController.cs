using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels;
using oop_s2_2_mvc_78286.Data;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Restricts the entire controller to Admin and Inspector roles by default
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class InspectionsController : Controller
    {
        private readonly ILogger<InspectionsController> _logger;
        private readonly ApplicationDbContext _context;

        public InspectionsController(ILogger<InspectionsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Inspections
        // Overrides class-level restriction to allow Viewers to see the list
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            // Includes Premises data for the view as per scaffolded logic
            var applicationDbContext = _context.Inspections.Include(i => i.Premises);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Inspections/Details/5
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();

            return View(inspection);
        }

        // GET: Inspections/Create
        public IActionResult Create()
        {
            // Populate dropdown for Premises selection
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name");
            return View();
        }

        // POST: Inspections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Business Rule Warning: Log if date is in the future
                    if (inspection.InspectionDate > DateTime.Now)
                    {
                        _logger.LogWarning("Validation Issue: Inspection date {Date} is in the future for Premises {Id}",
                            inspection.InspectionDate, inspection.PremisesId);
                    }

                    _context.Add(inspection);
                    await _context.SaveChangesAsync();

                    // Structured Information Log
                    _logger.LogInformation("Inspection created. ID: {InspectionId}, Premises: {PremisesId}",
                        inspection.Id, inspection.PremisesId);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Error Log with exception details
                _logger.LogError(ex, "Error occurred while creating inspection for Premises {Id}", inspection.PremisesId);
                return View("Error");
            }

            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        // GET: Inspections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return NotFound();

            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        // POST: Inspections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Inspection updated. ID: {InspectionId}", inspection.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
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

        // GET: Inspections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();

            return View(inspection);
        }

        // POST: Inspections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection deleted. ID: {Id}", id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}