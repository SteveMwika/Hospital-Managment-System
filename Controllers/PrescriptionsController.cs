using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace Hospital_Managment_System.Controllers
{
    [Authorize(Roles = "Doctor, Patient, Admin")]
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PrescriptionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Fetch all patient names
        [HttpGet]
        public async Task<IActionResult> GetPatientSuggestions()
        {
            var patients = await _context.Patients
                .Select(p => new { value = p.FirstName + " " + p.LastName })
                .ToListAsync();

            return Json(patients);
        }

        // Fetch all doctor names
        [HttpGet]
        public async Task<IActionResult> GetDoctorSuggestions()
        {
            var doctors = await _context.Doctors
                .Select(d => new { value = d.FirstName + " " + d.LastName })
                .ToListAsync();

            return Json(doctors);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminViewPrescriptions(string searchQuery, string filterBy)
        {
            // Fetch all doctors and patients for autofill
            var doctors = await _context.Doctors.Select(d => new { d.Id, FullName = d.FirstName + " " + d.LastName }).ToListAsync();
            var patients = await _context.Patients.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }).ToListAsync();

            // Pass doctors and patients to ViewBag for autofill
            ViewBag.Doctors = doctors;
            ViewBag.Patients = patients;

            // Fetch prescriptions with related data
            var prescriptions = _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .AsQueryable();

            // Filter prescriptions based on search query and filter type
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (filterBy == "Doctor")
                {
                    prescriptions = prescriptions.Where(p =>
                        (p.Doctor.FirstName + " " + p.Doctor.LastName).Contains(searchQuery));
                }
                else if (filterBy == "Patient")
                {
                    prescriptions = prescriptions.Where(p =>
                        (p.Patient.FirstName + " " + p.Patient.LastName).Contains(searchQuery));
                }
            }

            var result = await prescriptions.ToListAsync();

            // Pass filter options and prescriptions to the view
            ViewBag.FilterBy = filterBy;
            ViewBag.SearchQuery = searchQuery;

            return View("AdminViewPrescriptions", result);
        }

        // GET: Prescription/PatientPrescriptions (For Patient)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PatientPrescriptions()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return Forbid();
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.Doctor)
                .Where(p => p.PatientId == patient.Id)
                .ToListAsync();

            return View(prescriptions);
        }

        // GET: Prescription/DoctorPatients (For Doctor)
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorPatients()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return Forbid();
            }

            var patientsWithPrescriptions = await _context.Prescriptions
                .Include(p => p.Patient)
                .Where(p => p.DoctorId == doctor.Id)
                .Select(p => p.Patient)
                .Distinct()
                .ToListAsync();

            return View(patientsWithPrescriptions);
        }

        // GET: Prescription/PatientPrescriptions/5 (For Doctor to View a Specific Patient's Prescriptions)
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ViewPatientPrescriptions(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return Forbid();
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Where(p => p.PatientId == id && p.DoctorId == doctor.Id)
                .ToListAsync();

            ViewBag.PatientFullName = $"{patient.FirstName} {patient.LastName}";
            return View(prescriptions);
        }



        // GET: Prescriptions/AssignPrescription/5
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AssignPrescription(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            var medicines = await _context.Medicines.ToListAsync();
            ViewBag.Medicines = medicines;
            ViewBag.AppointmentId = appointmentId;

            return View();
        }

        // POST: Prescriptions/AssignPrescription
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPrescription(int appointmentId, int medicineId, string instructions)
        {
            // Fetch appointment details
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctors)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Fetch medicine details
            var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId);
            if (medicine == null)
            {
                return NotFound();
            }

            // Check if medicine stock is sufficient
            if (medicine.Quantity <= 0)
            {
                ModelState.AddModelError("", "Insufficient medicine stock.");
                return RedirectToAction("AssignPrescription", new { appointmentId });
            }

            // Fetch the doctor creating the prescription
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == _userManager.GetUserId(User));

            if (doctor == null)
            {
                return NotFound();
            }

            // Create the prescription
            var prescription = new Prescription
            {
                AppointmentId = appointmentId,
                MedicineId = medicineId,
                Instructions = instructions,
                DoctorId = doctor.Id,
                PatientId = appointment.PatientId
            };

            // Update medicine stock
            medicine.Quantity -= 1; // Adjust based on prescription quantity
            _context.Medicines.Update(medicine);

            // Log inventory change
            var inventoryLog = new MedicineInventoryLog
            {
                MedicineId = medicineId,
                ChangeType = MedicineChangeType.Prescribed,
                QuantityChanged = 1, // Change this to the actual quantity if needed
                NewQuantity = medicine.Quantity,
                DateTime = DateTime.Now,
                DoctorId = doctor.Id,
                PatientId = appointment.PatientId,
                Description = $"Prescription for patient {appointment.Patient.FirstName} {appointment.Patient.LastName}",
                AdminUserId = null // Explicitly set to null since this action is by a doctor
            };

            _context.MedicineInventoryLogs.Add(inventoryLog);

            // Save prescription
            _context.Prescriptions.Add(prescription);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Appointments", new { id = appointmentId });
        }


    }
}

   
