using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Hospital_Managment_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hospital_Managment_System.Controllers
{
    [Authorize]
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PrescriptionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Prescriptions
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            IQueryable<Prescription> prescriptionsQuery = _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .AsNoTracking();

            if (roles.Contains(UserRoles.Doctor.ToString()))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor != null)
                {
                    prescriptionsQuery = prescriptionsQuery.Where(p => p.DoctorId == doctor.Id);
                }
            }
            else if (roles.Contains(UserRoles.Patient.ToString()))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (patient != null)
                {
                    prescriptionsQuery = prescriptionsQuery.Where(p => p.PatientId == patient.Id);
                }
            }

            var prescriptions = await prescriptionsQuery.AsNoTracking().ToListAsync();
            return View(prescriptions);
        }

        // GET: Prescriptions/Details/5
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prescription == null)
            {
                return NotFound();
            }

            // Authorization check: Patients and Doctors can only access their own records
            if (!IsAuthorizedToAccessPrescription(prescription))
                return Forbid();

            return View(prescription);
        }

        // GET: Prescriptions/Create
        [Authorize(Roles = "Admin,Doctor")]
        public IActionResult Create()
        {
            // Populate Medicines dropdown
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "Id", "Name");

            // Populate Patients dropdown
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("Admin"))
            {
                // Admin can see all patients
                ViewData["PatientId"] = new SelectList(_context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName");
            }
            else if (User.IsInRole("Doctor"))
            {
                // Doctor can see only their own patients
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    ViewData["PatientId"] = new SelectList(
                        _context.Patients
                            .Where(p => p.PrimaryDoctorId == doctor.Id)
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                        "Id",
                        "FullName");
                }
                else
                {
                    ViewData["PatientId"] = new SelectList(Enumerable.Empty<SelectListItem>());
                }
            }

            // Populate Doctors dropdown (for Admin only)
            if (User.IsInRole("Admin"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName");
            }

            // Populate Appointment dropdown
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id"); // Adjust as needed

            return View();
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var prescription = new Prescription
                {
                    AppointmentId = model.AppointmentID,
                    MedicineId = model.MedicineID,
                    Instructions = model.Instructions,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns in case of error
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "Id", "Name", model.MedicineID);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("Admin"))
            {
                ViewData["PatientId"] = new SelectList(_context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", model.PatientId);
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", model.DoctorId);
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    ViewData["PatientId"] = new SelectList(
                        _context.Patients
                            .Where(p => p.PrimaryDoctorId == doctor.Id)
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                        "Id",
                        "FullName",
                        model.PatientId);
                }
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", model.AppointmentID);

            return View(model);
        }

        // GET: Prescriptions/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            // Authorization check
            if (!IsAuthorizedToAccessPrescription(prescription))
                return Forbid();

            // Populate dropdowns
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "Id", "Name", prescription.MedicineId);

            if (User.IsInRole("Admin"))
            {
                ViewData["PatientId"] = new SelectList(_context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", prescription.PatientId);
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", prescription.DoctorId);
            }
            else if (User.IsInRole("Doctor"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    ViewData["PatientId"] = new SelectList(
                        _context.Patients
                            .Where(p => p.PrimaryDoctorId == doctor.Id)
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                        "Id",
                        "FullName",
                        prescription.PatientId);
                }
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", prescription.AppointmentId);

            // Initialize ViewModel
            var model = new PrescriptionViewModel
            {
                Id = prescription.Id,
                AppointmentID = prescription.AppointmentId,
                MedicineID = prescription.MedicineId,
                Instructions = prescription.Instructions,
                DoctorId = prescription.DoctorId,
                PatientId = prescription.PatientId
            };

            return View(model);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, PrescriptionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Authorization check
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            if (!IsAuthorizedToAccessPrescription(prescription))
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    prescription.AppointmentId = model.AppointmentID;
                    prescription.MedicineId = model.MedicineID;
                    prescription.Instructions = model.Instructions;
                    prescription.DoctorId = model.DoctorId;
                    prescription.PatientId = model.PatientId;

                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(model.Id))
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

            // Repopulate dropdowns in case of error
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "Id", "Name", model.MedicineID);

            if (User.IsInRole("Admin"))
            {
                ViewData["PatientId"] = new SelectList(_context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", model.PatientId);
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", model.DoctorId);
            }
            else if (User.IsInRole("Doctor"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    ViewData["PatientId"] = new SelectList(
                        _context.Patients
                            .Where(p => p.PrimaryDoctorId == doctor.Id)
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                        "Id",
                        "FullName",
                        model.PatientId);
                }
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", model.AppointmentID);

            return View(model);
        }

        // GET: Prescriptions/Delete/5
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prescription == null)
            {
                return NotFound();
            }

            // Authorization check
            if (!IsAuthorizedToAccessPrescription(prescription))
                return Forbid();

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            // Authorization check
            if (!IsAuthorizedToAccessPrescription(prescription))
                return Forbid();

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.Id == id);
        }

        /// <summary>
        /// Checks if the current user is authorized to access the given prescription.
        /// </summary>
        /// <param name="prescription"></param>
        /// <returns></returns>
        private bool IsAuthorizedToAccessPrescription(Prescription prescription)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var roles = _userManager.GetRolesAsync(user).Result;

            if (roles.Contains(UserRoles.Admin.ToString()))
                return true;

            if (roles.Contains(UserRoles.Doctor.ToString()))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                return prescription.DoctorId == doctor?.Id;
            }

            if (roles.Contains(UserRoles.Patient.ToString()))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                return prescription.PatientId == patient?.Id;
            }

            return false;
        }
    }
}
