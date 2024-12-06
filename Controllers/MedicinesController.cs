//using Hospital_Managment_System.Data;
//using Hospital_Managment_System.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace Hospital_Managment_System.Controllers
//{
//    public class MedicinesController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public MedicinesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // View All Medicines
//        public async Task<IActionResult> Index()
//        {
//            var medicines = await _context.Medicines.ToListAsync();
//            return View(medicines);
//        }

//        // Create Medicine - GET
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // Create Medicine - POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Medicine medicine)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Medicines.Add(medicine);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(medicine);
//        }

//        // Edit Medicine - GET
//        public async Task<IActionResult> Edit(int id)
//        {
//            var medicine = await _context.Medicines.FindAsync(id);
//            if (medicine == null)
//            {
//                return NotFound();
//            }
//            return View(medicine);
//        }

//        // Edit Medicine - POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Medicine medicine)
//        {
//            if (id != medicine.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                _context.Update(medicine);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(medicine);
//        }

//        // Delete Medicine - GET
//        public async Task<IActionResult> Delete(int id)
//        {
//            var medicine = await _context.Medicines.FindAsync(id);
//            if (medicine == null)
//            {
//                return NotFound();
//            }
//            return View(medicine);
//        }

//        // Delete Medicine - POST
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var medicine = await _context.Medicines.FindAsync(id);
//            if (medicine != null)
//            {
//                _context.Medicines.Remove(medicine);
//                await _context.SaveChangesAsync();
//            }
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
