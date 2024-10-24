using Hospital_Managment_System.Models;
using Hospital_Managment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Hospital_Managment_System.Controllers
{
    [Authorize(Roles = "Doctor, Admin")]
    public class LabTestsController : Controller
    {
        private readonly ILabTestService _labTestService;

        public LabTestsController(ILabTestService labTestService)
        {
            _labTestService = labTestService;
        }

        // GET: LabTests
        public async Task<IActionResult> Index()
        {
            var labTests = await _labTestService.GetAllLabTestsAsync();
            return View(labTests);
        }

        // GET: LabTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _labTestService.GetLabTestByIdAsync(id.Value);
            if (labTest == null)
            {
                return NotFound();
            }

            return View(labTest);
        }

        // GET: LabTests/Create
        [Authorize(Roles = "Doctor")]
        public IActionResult Create(int appointmentId)
        {
            var labTest = new LabTest
            {
                AppointmentId = appointmentId,
                TestDate = DateTime.UtcNow,
                IsCompleted = false
            };
            return View(labTest);
        }

        // POST: LabTests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create([Bind("AppointmentId,TestName,TestResult,TestDate,IsCompleted,Comments")] LabTest labTest)
        {
            if (!ModelState.IsValid)
            {
                await _labTestService.CreateLabTestAsync(labTest);
                return RedirectToAction(nameof(Index));
            }
            return View(labTest);
        }

        // GET: LabTests/Edit/5
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _labTestService.GetLabTestByIdAsync(id.Value);
            if (labTest == null)
            {
                return NotFound();
            }
            return View(labTest);
        }

        // POST: LabTests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentId,TestName,TestResult,TestDate,IsCompleted,Comments")] LabTest labTest)
        {
            if (id != labTest.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _labTestService.UpdateLabTestAsync(labTest);
                }
                catch (Exception)
                {
                    // Handle exceptions (e.g., concurrency issues)
                    return BadRequest();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(labTest);
        }

        // GET: LabTests/Delete/5
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _labTestService.GetLabTestByIdAsync(id.Value);
            if (labTest == null)
            {
                return NotFound();
            }

            return View(labTest);
        }

        // POST: LabTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _labTestService.DeleteLabTestAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
