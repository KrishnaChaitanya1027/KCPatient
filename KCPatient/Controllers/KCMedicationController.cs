using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KCPatient.Models;
using Microsoft.AspNetCore.Http;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace KCPatient.Controllers
{
    public class KCMedicationController : Controller
    {
        private readonly PatientContext _context;
        private readonly INotyfService _notyfService;

        public KCMedicationController(PatientContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: KCMedication
        public async Task<IActionResult> Index(int id, string medName)
        {

            // evaluating the id > 0 with setting the value to session
            if (id > 0)
            {
                HttpContext.Session.SetInt32("medicationTypeId", id);
                HttpContext.Session.SetString("medicationName", medName);
            }


            //Getting the value from the session
            var medId = HttpContext.Session.GetInt32("medicationTypeId");


            //If medication ID is not found then shows the error message with medication type controller
            if (medId == 0 || medId == null)
            {
                TempData["medicationTypeError"] = "true";
               
                return RedirectToAction("Index", "KCMedicationType");
            }
            //Session["medicationTypeId"] = id;
            var patientContext = _context.Medication.Where(x=>x.MedicationTypeId == medId).OrderBy(x=>x.MedicationType).ThenBy(x=>x.Concentration).Include(m => m.ConcentrationCodeNavigation).Include(m => m.DispensingCodeNavigation).Include(m => m.MedicationType);
            return View(await patientContext.ToListAsync());
        }

        // GET: KCMedication/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: KCMedication/Create
        public IActionResult Create()
        {
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode");
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode");
            return View();
        }

        // POST: KCMedication/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Din,Name,Image,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            if (ModelState.IsValid)
            {
                medication.MedicationTypeId = HttpContext.Session.GetInt32("medicationTypeId").Value;

                var alreadyExists = _context.Medication.FirstOrDefault(x => x.Name == medication.Name && x.ConcentrationCode == medication.ConcentrationCode && x.Concentration == medication.Concentration);
                if(alreadyExists != null)
                {
                    //_notyfService.Error("Medication with same values already exists");
                    TempData["message"] = "Medication with same values already exists";
                }
                else
                {
                    _context.Add(medication);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("You have successfully saved the data.");
                    return RedirectToAction(nameof(Index));
                }

               
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
            return View(medication);
        }

        // GET: KCMedication/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
            return View(medication);
        }

        // POST: KCMedication/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Din,Name,Image,MedicationTypeId,DispensingCode")] Medication medication)
        {
            if (id != medication.Din)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var data = _context.Medication.AsNoTracking().Where(x => x.Din == medication.Din).FirstOrDefault();
                    if (data != null)
                    {
                        medication.Concentration = data.Concentration;
                        medication.ConcentrationCode = data.ConcentrationCode;
                    }

                    var alreadyExists = _context.Medication.FirstOrDefault(x => x.Name == medication.Name && x.ConcentrationCode == medication.ConcentrationCode && x.Concentration == medication.Concentration && x.Din != medication.Din);
                    if (alreadyExists != null)
                    {
                        //_notyfService.Error("Medication with same values already exists");
                        TempData["message"] = "Medication with same values already exists";
                        ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
                        ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
                        return View(medication);
                    }
                    else
                    {
                        _context.Update(medication);
                        await _context.SaveChangesAsync();
                        _notyfService.Success("Record updated successfully");
                        return RedirectToAction(nameof(Index));
                    }
                   

                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Din))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
            return View(medication);
        }

        // GET: KCMedication/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // POST: KCMedication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var medication = await _context.Medication.FindAsync(id);
            _context.Medication.Remove(medication);
            await _context.SaveChangesAsync();
            _notyfService.Success("Record deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        private bool MedicationExists(string id)
        {
            return _context.Medication.Any(e => e.Din == id);
        }
    }
}
