using FoodSafety.Domain.Models;
using FoodSafety.Domain.Models.ViewModels;
using Microsoft.AspNetCore.Authorization; // Required for Role-Based Access 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78286.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Restricts the entire controller to Admin and Inspector roles by default
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger; // Injected for Serilog 

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FollowUps
        // Allows Viewers to see the list of follow-up tasks
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FollowUps.Include(f => f.Inspection);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FollowUps/Details/5
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // GET: FollowUps/Create
        public IActionResult Create()
        {
            // Load Inspections including Premises so we can show the name in the dropdown
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();

            // We format the Display Text so the JavaScript in the View can parse it
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText");

            return View();
        }

        // --- NEW: GET: FollowUps/Edit/5
        // Added because the Edit view requires a GET action that loads the model and populates the dropdown.
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            // Populate dropdown with the same formatted display text used by Create
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                    // STRICT SPEC: Due Date must be after Inspection Date
                    if (inspection != null && followUp.DueDate <= inspection.InspectionDate)
                    {
                        _logger.LogWarning("Validation Failure: DueDate {Due} is not after InspectionDate {Insp}",
                            followUp.DueDate, inspection.InspectionDate);
                        ModelState.AddModelError("DueDate", $"Due Date must be after the Inspection Date ({inspection.InspectionDate.ToShortDateString()})");
                    }
                    else
                    {
                        _context.Add(followUp);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Follow-up created for Inspection {Id}", followUp.InspectionId);
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating follow-up");
                // REQUIREMENT: Redirect to Global Error Page
                return RedirectToAction("Error", "Home");
            }

            // If we reach here, re-populate the dropdown with the same formatting
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            try
            {
                // 1. Validate the model (this triggers IValidatableObject logic)
                if (ModelState.IsValid)
                {
                    // 2. Fetch inspection to verify business rules
                    var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                    if (inspection != null && followUp.DueDate <= inspection.InspectionDate)
                    {
                        ModelState.AddModelError("DueDate", $"Due Date must be after the Inspection Date ({inspection.InspectionDate.ToShortDateString()})");
                    }
                    else
                    {
                        _context.Update(followUp);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Follow-up {Id} updated successfully.", followUp.Id);
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowUpExists(followUp.Id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating follow-up {Id}", id);
                // Instead of RedirectToAction, return the Error view directly WITH a new model
                // This prevents the NullReferenceException in Error.cshtml
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message
                });
            }

            // --- IMPORTANT: RE-POPULATE DROPDOWN WITH PREMISES ---
            // If validation fails, we must provide the same formatted text used in GET Edit
            var inspections = _context.Inspections.Include(i => i.Premises).ToList();
            ViewBag.InspectionId = new SelectList(inspections.Select(i => new {
                Id = i.Id,
                DisplayText = $"ID: {i.Id} - {i.Premises.Name} ({i.InspectionDate.ToShortDateString()})"
            }), "Id", "DisplayText", followUp.InspectionId);

            return View(followUp);
        }
        

        // GET: FollowUps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
                _logger.LogWarning("Follow-up {Id} was deleted by user.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}