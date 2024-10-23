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

        public AppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager) // Modify constructor
        {
            _context = context;
            _userManager = userManager;
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


        // GET: Appointments/Create
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.Id;

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUserId);
                if (doctor == null)
                {
                    ModelState.AddModelError("", "Doctor not found.");
                    return View();
                }

                // Show only the patients assigned to this doctor
                ViewBag.PatientId = new SelectList(_context.Patients
                    .Where(p => p.PrimaryDoctorId == doctor.Id)
                    .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName");
            }
            else if (User.IsInRole("Admin"))
            {
                // Admin selects both Patient and Doctor from dropdowns
                ViewBag.PatientId = new SelectList(_context.Patients
                    .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName");

                ViewBag.DoctorId = new SelectList(_context.Doctors
                    .Select(d => new { d.Id, FullName = d.FirstName + " " + d.LastName }), "Id", "FullName");
            }
            else if (User.IsInRole("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
                if (patient == null)
                {
                    ModelState.AddModelError("", "Patient not found.");
                    return View();
                }

                // PatientId is automatically assigned, no dropdown needed
            }

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Create([Bind("AppointmentDate,AppointmentStatus")] Appointment appointment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.Id;

            if (ModelState.IsValid)
            {
                // Assign BillStatus and BillAmount
                appointment.BillStatus = BillStatus.Unpaid.ToString();
                appointment.BillAmount = new Random().Next(50, 201); // $50 to $200

                if (User.IsInRole("Doctor"))
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUserId);
                    if (doctor == null)
                    {
                        ModelState.AddModelError("", "Doctor not found.");
                        // Repopulate ViewBag.PatientId before returning view
                        ViewBag.PatientId = new SelectList(_context.Patients
                            .Where(p => p.PrimaryDoctorId == doctor.Id)
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", appointment.PatientId);
                        return View(appointment);
                    }
                    appointment.DoctorId = doctor.Id;
                }
                else if (User.IsInRole("Patient"))
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
                    if (patient == null)
                    {
                        ModelState.AddModelError("", "Patient not found.");
                        return View(appointment);
                    }
                    appointment.PatientId = patient.Id;
                }
                else if (User.IsInRole("Admin"))
                {
                    // Admin selects both DoctorId and PatientId from dropdowns
                    // They are already bound from the form
                    // Validate if DoctorId is selected
                    if (appointment.DoctorId == 0)
                    {
                        ModelState.AddModelError("DoctorId", "Doctor is required.");
                        // Repopulate ViewBags
                        ViewBag.PatientId = new SelectList(_context.Patients
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", appointment.PatientId);

                        ViewBag.DoctorId = new SelectList(_context.Doctors
                            .Select(d => new { d.Id, FullName = d.FirstName + " " + d.LastName }), "Id", "FullName", appointment.DoctorId);
                        return View(appointment);
                    }
                }

                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, repopulate ViewBag
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUserId);
                ViewBag.PatientId = new SelectList(_context.Patients
                    .Where(p => p.PrimaryDoctorId == doctor.Id)
                    .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", appointment.PatientId);
            }
            else if (User.IsInRole("Admin"))
            {
                ViewBag.PatientId = new SelectList(_context.Patients
                    .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }), "Id", "FullName", appointment.PatientId);

                ViewBag.DoctorId = new SelectList(_context.Doctors
                    .Select(d => new { d.Id, FullName = d.FirstName + " " + d.LastName }), "Id", "FullName", appointment.DoctorId);
            }

            return View(appointment);
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
                // Doctor can view appointment details, patient name, and assigned doctors
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
                // Patient can view doctor name, appointment date, and feedback fields
                ViewBag.DoctorFullName = $"{appointment.Doctors.FirstOrDefault()?.FirstName} {appointment.Doctors.FirstOrDefault()?.LastName}";

                // Populate feedback status for patients
                ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>(), appointment.FeedbackStatus);
            }

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Edit(int id, Appointment appointment, string feedbackInput, int[] DoctorIds)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isDoctor = User.IsInRole("Doctor");
            var isPatient = User.IsInRole("Patient");

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingAppointment = await _context.Appointments
                        .Include(a => a.Doctors)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (existingAppointment == null)
                    {
                        return NotFound();
                    }

                    if (isDoctor)
                    {
                        // Doctor updates appointment status and doctors
                        existingAppointment.AppointmentDate = appointment.AppointmentDate;
                        existingAppointment.AppointmentStatus = appointment.AppointmentStatus;

                        // Remove all current doctors
                        existingAppointment.Doctors.Clear();

                        // Re-assign selected doctors
                        if (DoctorIds != null && DoctorIds.Length > 0)
                        {
                            foreach (var doctorId in DoctorIds)
                            {
                                var doctor = await _context.Doctors.FindAsync(doctorId);
                                if (doctor != null)
                                {
                                    existingAppointment.Doctors.Add(doctor);
                                }
                            }
                        }
                    }
                    else if (isPatient)
                    {
                        // Patient updates feedback and feedback status
                        existingAppointment.Feedback = feedbackInput;

                        // Update FeedbackStatus based on whether feedback is provided or not
                        if (!string.IsNullOrWhiteSpace(feedbackInput))
                        {
                            existingAppointment.FeedbackStatus = FeedbackStatus.Given;
                        }
                        else
                        {
                            existingAppointment.FeedbackStatus = FeedbackStatus.Pending;
                        }
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

            // Repopulate data in case of error
            if (isDoctor)
            {
                var assignedDoctors = appointment.Doctors.Select(d => d.Id).ToList();
                var unassignedDoctors = await _context.Doctors
                    .Where(d => !assignedDoctors.Contains(d.Id))
                    .OrderBy(d => d.FirstName).ThenBy(d => d.LastName)
                    .ToListAsync();

                ViewBag.AssignedDoctors = appointment.Doctors;
                ViewBag.UnassignedDoctors = unassignedDoctors;
            }
            else if (isPatient)
            {
                ViewBag.DoctorFullName = $"{appointment.Doctors.FirstOrDefault()?.FirstName} {appointment.Doctors.FirstOrDefault()?.LastName}";
            }

            return View(appointment);
        }


        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Billing)
                .Include(a => a.Doctors)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
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
