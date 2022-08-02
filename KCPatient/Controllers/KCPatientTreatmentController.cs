using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KCPatient.Models;
using Microsoft.AspNetCore.Http;

namespace KCPatient.Controllers
{
    public class KCPatientTreatmentController : Controller
    {
        private readonly PatientContext _context;

        public KCPatientTreatmentController(PatientContext context)
        {
            _context = context;
        }

        // GET: KCPatientTreatment
        public async Task<IActionResult> Index(int id, string diagnosis, string patientName)
        {
            if(id>0)
            {
                HttpContext.Session.SetInt32("diagId", id);
                HttpContext.Session.SetString("diagnosis", diagnosis);      
                HttpContext.Session.SetString("patientName", patientName);
            }

            var diagId = HttpContext.Session.GetInt32("diagId");
            if(diagId == 0 || diagId == null)
            {
                TempData["diagError"] = "true";
                return RedirectToAction("Index", "KCPatientDiagnosis");
            }

            var patientContext = _context.PatientTreatment.Where(x=>x.PatientDiagnosisId == diagId).Include(p => p.PatientDiagnosis).Include(p => p.Treatment).OrderByDescending(x=>x.PatientTreatmentId);
            return View(await patientContext.ToListAsync());
        }

        // GET: KCPatientTreatment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // GET: KCPatientTreatment/Create
        public IActionResult Create()
        {
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId");
            var diagId = HttpContext.Session.GetInt32("diagId");
            if(diagId == 0 || diagId == null)
            {
                TempData["diagError"] = "true";
                return RedirectToAction("Index", "KCPatientTreatment");
            }


            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(x=>x.DiagnosisId == diagId), "TreatmentId", "Name");
            PatientTreatment patientTreatment = new PatientTreatment();
            patientTreatment.DatePrescribed = DateTime.Now;
            return View(patientTreatment);
        }

        // POST: KCPatientTreatment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            var diagId = HttpContext.Session.GetInt32("diagId");
            if (diagId == 0 || diagId == null)
            {
                TempData["diagError"] = "true";
                return RedirectToAction("Index", "KCPatientTreatment");
            }
            if (ModelState.IsValid)
            {
                patientTreatment.PatientDiagnosisId = diagId.Value;
                _context.Add(patientTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            

            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(x => x.DiagnosisId == diagId), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // GET: KCPatientTreatment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            if (patientTreatment == null)
            {
                return NotFound();
            }
            var diagId = HttpContext.Session.GetInt32("diagId");
            if (diagId == 0 || diagId == null)
            {
                TempData["diagError"] = "true";
                return RedirectToAction("Index", "KCPatientTreatment");
            }
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(x => x.DiagnosisId == diagId), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // POST: KCPatientTreatment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            if (id != patientTreatment.PatientTreatmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientTreatmentExists(patientTreatment.PatientTreatmentId))
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
            var diagId = HttpContext.Session.GetInt32("diagId");
            if (diagId == 0 || diagId == null)
            {
                TempData["diagError"] = "true";
                return RedirectToAction("Index", "KCPatientTreatment");
            }
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(x=>x.DiagnosisId ==diagId), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // GET: KCPatientTreatment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // POST: KCPatientTreatment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            _context.PatientTreatment.Remove(patientTreatment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientTreatmentExists(int id)
        {
            return _context.PatientTreatment.Any(e => e.PatientTreatmentId == id);
        }
    }
}
