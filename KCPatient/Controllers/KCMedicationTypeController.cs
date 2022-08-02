using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KCPatient.Models;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace KCPatient.Controllers
{
    public class KCMedicationTypeController : Controller
    {
        private readonly PatientContext _context;
        private readonly INotyfService _notyfService;
        public KCMedicationTypeController(PatientContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: KCMedicationType
        public async Task<IActionResult> Index()
        {

            return View(await _context.MedicationType.ToListAsync());
        }

        // GET: KCMedicationType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType
                .FirstOrDefaultAsync(m => m.MedicationTypeId == id);
            if (medicationType == null)
            {
                return NotFound();
            }

            return View(medicationType);
        }

        // GET: KCMedicationType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KCMedicationType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedicationTypeId,Name")] MedicationType medicationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicationType);
                await _context.SaveChangesAsync();
                _notyfService.Success("Record added successfully");
                return RedirectToAction(nameof(Index));
            }
            return View(medicationType);
        }

        // GET: KCMedicationType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType.FindAsync(id);
            if (medicationType == null)
            {
                return NotFound();
            }
            return View(medicationType);
        }

        // POST: KCMedicationType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MedicationTypeId,Name")] MedicationType medicationType)
        {
            if (id != medicationType.MedicationTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicationType);
                    _notyfService.Success("Record updated successfully");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationTypeExists(medicationType.MedicationTypeId))
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
            return View(medicationType);
        }

        // GET: KCMedicationType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType
                .FirstOrDefaultAsync(m => m.MedicationTypeId == id);
            if (medicationType == null)
            {
                return NotFound();
            }

            return View(medicationType);
        }

        // POST: KCMedicationType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicationData = await _context.Medication.Where(x => x.MedicationTypeId == id).ToListAsync();
            var medicationType = await _context.MedicationType.FindAsync(id);

            foreach (var med in medicationData)
            {
                var medData = _context.PatientMedication.Where(x => x.Din == med.Din);
                var treatData = _context.TreatmentMedication.Where(x => x.Din == med.Din);
                _context.PatientMedication.RemoveRange(medData);
                _context.TreatmentMedication.RemoveRange(treatData);
            }

            _context.Medication.RemoveRange(medicationData);

            _context.MedicationType.Remove(medicationType);
            await _context.SaveChangesAsync();
            _notyfService.Success("Record deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        private bool MedicationTypeExists(int id)
        {
            return _context.MedicationType.Any(e => e.MedicationTypeId == id);
        }
    }
}
