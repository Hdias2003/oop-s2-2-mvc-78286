using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Required for Role-Based Access 
using oop_s2_2_mvc_78286.Models;
using oop_s2_2_mvc_78286.Data;

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
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id");
            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (ModelState.IsValid)
            {
                // Logic to warn if a due date is set in the past upon creation
                if (followUp.DueDate < DateTime.Now && followUp.Status == "Open")
                {
                    _logger.LogWarning("Creating an immediate overdue FollowUp for Inspection {Id}", followUp.InspectionId);
                }

                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Follow-up task created for Inspection ID: {InspectionId}", followUp.InspectionId);

                return RedirectToAction(nameof(Index));
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // GET: FollowUps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Business Logic: If status is set to Closed and no date provided, auto-fill it
                    if (followUp.Status == "Closed" && followUp.ClosedDate == null)
                    {
                        followUp.ClosedDate = DateTime.Now;
                        _logger.LogInformation("Follow-up {Id} automatically timestamped as closed.", followUp.Id);
                    }

                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Follow-up {Id} updated successfully.", followUp.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowUpExists(followUp.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
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