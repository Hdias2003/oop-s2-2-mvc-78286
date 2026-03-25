using Microsoft.AspNetCore.Mvc;
using oop_s2_2_mvc_78286.Models;

namespace oop_s2_2_mvc_78286.Controllers
{
    public class InspectionsController : Controller
    {
        private readonly ILogger<InspectionsController> _logger;
        private readonly ApplicationDbContext _context;

        public InspectionsController(ILogger<InspectionsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inspection inspection)
        {
            try
            {
                
                if (inspection.InspectionDate > DateTime.Now)
                {
                    _logger.LogWarning("Validation Issue: Inspection date {Date} is in the future for Premises {Id}",
                        inspection.InspectionDate, inspection.PremisesId);
                }

                _context.Add(inspection);
                await _context.SaveChangesAsync();

                
                _logger.LogInformation("Inspection created. ID: {InspectionId}, Premises: {PremisesId}",
                    inspection.Id, inspection.PremisesId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error occurred while creating inspection for Premises {Id}", inspection.PremisesId);
                return View("Error");
            }
        }
    }
}
