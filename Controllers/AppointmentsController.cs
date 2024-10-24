using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Hospital_Managment_System.Models.ViewModels;
using Hospital_Managment_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hospital_Managment_System.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<AppointmentsController> logger) // Modify constructor
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Appointments
        [Authorize(Roles = "Admin, Doctor, Patient")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IQueryable<Appointment> appointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctors)
                .AsNoTracking();

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                {
                    // Show all appointments related to the logged-in doctor
                    appointmentsQuery = appointmentsQuery.Where(a => a.Doctors.Any(d => d.Id == doctor.Id));
                }
            }
            else if (User.IsInRole("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient != null)
                {
                    // Show all appointments related to the logged-in patient
                    appointmentsQuery = appointmentsQuery.Where(a => a.PatientId == patient.Id);
                }
            }

            var appointments = await appointmentsQuery.ToListAsync();
            return View(appointments);
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Admin, Doctor, Patient")] // Allow only Admin, Doctor, and Patient to view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // Find the current user's identity
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Billing)
                .Include(a => a.Doctors)
                .Include(a => a.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Include(a => a.LabTests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Check if the current user is an Admin
            if (User.IsInRole("Admin"))
            {
                return View(appointment); // Admin can see everything
            }

            // Check if the current user is a Doctor
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUserId);
                if (doctor == null || !appointment.Doctors.Any(d => d.Id == doctor.Id))
                {
                    return Forbid(); // Ensure that the doctor is assigned to this appointment
                }
                return View(appointment); // Doctor can see everything except billing information
            }

            // Check if the current user is a Patient
            if (User.IsInRole("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
                if (patient == null || appointment.PatientId != patient.Id)
                {
                    return Forbid(); // Ensure that the appointment belongs to the logged-in patient
                }
                return View(appointment); // Patient can see everything including billing
            }

            return Forbid(); // Default forbid if no valid role matches
        }

        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Check if the user is a Doctor
            if (User.IsInRole("Doctor"))
            {
                // Get the doctor record based on the logged-in user
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
                if (doctor == null)
                {
                    return NotFound("Doctor not found.");
                }

                // Populate the patients list with only the doctor's patients
                var patientsList = await _context.Patients
                    .Where(p => p.PrimaryDoctorId == doctor.Id)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.FirstName} {p.LastName}"
                    }).ToListAsync();

                ViewBag.PatientsList = patientsList;
                ViewBag.DoctorName = $"{doctor.FirstName} {doctor.LastName}";
                ViewBag.CurrentDoctorId = doctor.Id;
                ViewBag.AppointmentStatus = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>(), "Pending");

            }
            else if (User.IsInRole("Patient"))
            {
                // Get the patient record based on the logged-in user
                var patient = await _context.Patients.Include(p => p.PrimaryDoctor)
                                                     .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                // Populate the doctors list with all doctors, with the primary doctor pre-selected if applicable
                var doctorsList = await _context.Doctors
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.FirstName} {d.LastName}",
                        Selected = d.Id == patient.PrimaryDoctorId
                    }).ToListAsync();

                ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
                ViewBag.DoctorsList = doctorsList;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Create(Appointment appointment, int selectedDoctorId, int? selectedPatientId)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(appointment);
            //}

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // If the current user is a doctor
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
                if (doctor == null)
                {
                    return NotFound("Doctor not found.");
                }

                if (selectedPatientId == null)
                {
                    ModelState.AddModelError("selectedPatientId", "You must select a patient.");
                    return View(appointment);
                }

                // Assign the selected patient to the appointment
                var patient = await _context.Patients.FindAsync(selectedPatientId);
                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                appointment.PatientId = patient.Id;
                appointment.Doctors.Add(doctor);  // Doctor assigning themselves
            }
            else if (User.IsInRole("Patient"))
            {
                // Get the patient associated with the logged-in user
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                // Set properties of the appointment
                appointment.PatientId = patient.Id;
                appointment.BillAmount = GenerateRandomBillAmount();
                appointment.DoctorNotification = (int)NotificationStatus.Unread;
                appointment.PatientNotification = (int)NotificationStatus.Unread;
                appointment.FeedbackStatus = FeedbackStatus.Pending;
                appointment.Feedback = string.Empty;
                appointment.AppointmentStatus = AppointmentStatus.Pending;

                // Assign the selected doctor to the appointment
                var doctor = await _context.Doctors.FindAsync(selectedDoctorId);
                if (doctor != null)
                {
                    appointment.Doctors.Add(doctor);
                }
            }

            // Save the appointment
            _context.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private float GenerateRandomBillAmount()
        {
            Random rand = new Random();
            return (float)(rand.NextDouble() * (300 - 100) + 100);
        }


        // GET: Appointments/Edit/5
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctors)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isDoctor = User.IsInRole("Doctor");
            var isPatient = User.IsInRole("Patient");

            if (isDoctor)
            {
                // Doctor view: see patient info and manage doctors
                var assignedDoctors = appointment.Doctors.Select(d => d.Id).ToList();
                var unassignedDoctors = await _context.Doctors
                    .Where(d => !assignedDoctors.Contains(d.Id))
                    .OrderBy(d => d.FirstName).ThenBy(d => d.LastName)
                    .ToListAsync();

                ViewBag.AssignedDoctors = appointment.Doctors;
                ViewBag.UnassignedDoctors = unassignedDoctors;
                ViewBag.PatientFullName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";

                // Populate appointment status for doctors
                ViewData["AppointmentStatus"] = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>(), appointment.AppointmentStatus);
            }
            else if (isPatient)
            {
                // Patient view: view doctor names and manage feedback
                ViewBag.DoctorFullName = appointment.Doctors
                    .Select(d => $"{d.FirstName} {d.LastName}")
                    .ToList();

                // Populate feedback status for patients
                ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>(), appointment.FeedbackStatus);
            }

            return View(appointment);
        }


        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Edit(int id, Appointment appointment, string feedbackInput, string DoctorIds)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isDoctor = User.IsInRole("Doctor");
            var isPatient = User.IsInRole("Patient");

            try
            {
                var existingAppointment = await _context.Appointments
                    .Include(a => a.Doctors)  // Ensure we load existing doctors
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (existingAppointment == null)
                {
                    return NotFound();
                }

                if (isDoctor)
                {
                    // Update appointment date and status
                    existingAppointment.AppointmentDate = appointment.AppointmentDate;
                    existingAppointment.AppointmentStatus = appointment.AppointmentStatus;

                    // Update doctor assignments
                    if (!string.IsNullOrEmpty(DoctorIds))
                    {
                        var doctorIdArray = DoctorIds.Split(',').Select(int.Parse).ToArray();

                        // Remove unselected doctors
                        var doctorsToRemove = existingAppointment.Doctors.Where(d => !doctorIdArray.Contains(d.Id)).ToList();
                        foreach (var doctor in doctorsToRemove)
                        {
                            existingAppointment.Doctors.Remove(doctor);
                        }

                        // Add newly selected doctors
                        foreach (var doctorId in doctorIdArray)
                        {
                            if (!existingAppointment.Doctors.Any(d => d.Id == doctorId))
                            {
                                var doctor = await _context.Doctors.FindAsync(doctorId);
                                if (doctor != null)
                                {
                                    existingAppointment.Doctors.Add(doctor);
                                }
                            }
                        }
                    }
                }
                else if (isPatient)
                {
                    // Update feedback and feedback status
                    existingAppointment.Feedback = feedbackInput;
                    existingAppointment.FeedbackStatus = !string.IsNullOrWhiteSpace(feedbackInput)
                        ? FeedbackStatus.Given
                        : FeedbackStatus.Pending;

                    // Patient can also update the appointment date
                    existingAppointment.AppointmentDate = appointment.AppointmentDate;
                }

                _context.Update(existingAppointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
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








        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment != null)
            {
                // Remove associations with Doctors
                appointment.Doctors.Clear();

                // Remove Billing if exists
                var billing = await _context.Billings.FirstOrDefaultAsync(b => b.AppointmentId == id);
                if (billing != null)
                {
                    _context.Billings.Remove(billing);
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
