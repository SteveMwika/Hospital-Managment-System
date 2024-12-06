using Microsoft.AspNetCore.Mvc;
using Hospital_Managment_System.Data; // Assuming this contains DbContext
using Hospital_Managment_System.Models;
using System.Linq;
using System.Threading.Tasks;
using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managment_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MedicineInventoryLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MedicineInventoryLogsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medicines (Index)
        public async Task<IActionResult> Index()
        {
            var medicines = await _context.Medicines
                .OrderBy(m => m.Id)
                .ToListAsync();

            return View(medicines); // View shows the list of medicines with available quantities
        }

        // GET: MedicineInventoryLogs
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InventoryLogs(string changeType, string patientName, string doctorId)
        {
            // Fetch all logs
            var logs = _context.MedicineInventoryLogs
                .Include(log => log.Medicine)
                .Include(log => log.Doctor)
                .Include(log => log.Patient)
                .Include(log => log.AdminUser)
                .AsQueryable();

            // Filter by Medicine Change Type
            if (!string.IsNullOrEmpty(changeType) && Enum.TryParse<MedicineChangeType>(changeType, out var parsedChangeType))
            {
                logs = logs.Where(log => log.ChangeType == parsedChangeType);
            }

            // Filter by Patient Name
            if (!string.IsNullOrEmpty(patientName))
            {
                logs = logs.Where(log =>
                    log.Patient != null &&
                    (log.Patient.FirstName + " " + log.Patient.LastName).Contains(patientName));
            }

            // Filter by Doctor ID
            if (!string.IsNullOrEmpty(doctorId))
            {
                logs = logs.Where(log => log.Doctor != null && log.Doctor.Id.ToString() == doctorId);
            }

            // Fetch filtered logs
            var filteredLogs = await logs.OrderByDescending(log => log.DateTime).ToListAsync();

            // Pass filter data for autofill
            ViewBag.ChangeTypes = Enum.GetNames(typeof(MedicineChangeType)).ToList();
            ViewBag.Doctors = await _context.Doctors.Select(d => new { d.Id, FullName = d.FirstName + " " + d.LastName }).ToListAsync();
            ViewBag.Patients = await _context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }).ToListAsync();

            return View(filteredLogs);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorSuggestions(string term)
        {
            var doctors = await _context.Doctors
                .Where(d => (d.FirstName + " " + d.LastName).Contains(term))
                .Select(d => new { value = d.FirstName + " " + d.LastName })
                .ToListAsync();

            return Json(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientSuggestions(string term)
        {
            var patients = await _context.Patients
                .Where(p => (p.FirstName + " " + p.LastName).Contains(term))
                .Select(p => new { value = p.FirstName + " " + p.LastName })
                .ToListAsync();

            return Json(patients);
        }



        // GET: Add to Inventory
        public async Task<IActionResult> AddToInventory(int medicineId)
        {
            var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine); // Pass the selected medicine to the view
        }

        // POST: Add to Inventory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToInventory(int medicineId, int quantityAdded, string description)
        {
            if (quantityAdded <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0.");
                var selectedMedicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId);
                return View(selectedMedicine);
            }

            var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId);
            if (medicine == null)
            {
                return NotFound();
            }

            // Update medicine stock
            medicine.Quantity += quantityAdded;
            _context.Medicines.Update(medicine);

            // Log inventory change
            var adminUserId = _userManager.GetUserId(User);
            var log = new MedicineInventoryLog
            {
                MedicineId = medicineId,
                ChangeType = MedicineChangeType.Added,
                QuantityChanged = quantityAdded,
                NewQuantity = medicine.Quantity,
                DateTime = DateTime.Now,
                AdminUserId = adminUserId,
                Description = description ?? $"Added {quantityAdded} units to inventory."
            };
            _context.MedicineInventoryLogs.Add(log);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
