using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Required for security
using oop_s2_2_mvc_78286.Models;
using oop_s2_2_mvc_78286.Data;

namespace oop_s2_2_mvc_78286.Controllers
{
    // Restricts the entire controller to Admin and Inspector roles by default
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector)]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger; // Logger for Serilog

        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Premises
        // Overrides class restriction to allow Viewers to see the list
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Premises.ToListAsync());
        }

        // GET: Premises/Details/5
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Inspector + "," + UserRoles.Viewer)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premises = await _context.Premises
                .FirstOrDefaultAsync(m => m.Id == id);
            if (premises == null)
            {
                return NotFound();
            }

            return View(premises);
        }

        // GET: Premises/Create
        // Inherits [Authorize] from class level (Admin/Inspector only)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Premises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (ModelState.IsValid)
            {
                _context.Add(premises);
                await _context.SaveChangesAsync();

                // Log the creation for the audit trail
                _logger.LogInformation("New Premises created: {Name} in {Town} with Risk Level {Risk}",
                    premises.Name, premises.Town, premises.RiskRating);

                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        // GET: Premises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premises = await _context.Premises.FindAsync(id);
            if (premises == null)
            {
                return NotFound();
            }
            return View(premises);
        }

        // POST: Premises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (id != premises.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(premises);
                    await _context.SaveChangesAsync();

                    // Log the update
                    _logger.LogInformation("Premises updated: ID {Id}, Name {Name}", premises.Id, premises.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PremisesExists(premises.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        // GET: Premises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premises = await _context.Premises
                .FirstOrDefaultAsync(m => m.Id == id);
            if (premises == null)
            {
                return NotFound();
            }

            return View(premises);
        }

        // POST: Premises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises != null)
            {
                _context.Premises.Remove(premises);
                await _context.SaveChangesAsync();

                // Log the deletion
                _logger.LogWarning("Premises deleted: ID {Id}, Name {Name}", id, premises.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PremisesExists(int id)
        {
            return _context.Premises.Any(e => e.Id == id);
        }
    }
}