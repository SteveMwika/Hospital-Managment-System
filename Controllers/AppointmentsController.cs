using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
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
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Billing)
                .Include(a => a.Doctors)
                .Include(a => a.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Include(a => a.LabTests)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Doctor"))
            {
                // If the current user is a Doctor, they can only create an appointment for themselves
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
                if (doctor == null)
                {
                    return Forbid(); // Ensure that the doctor can only create their own appointments
                }

                // Populate Patients dropdown (Only patients of the doctor can be selected)
                var patients = await _context.Patients.Where(p => p.PrimaryDoctorId == doctor.Id).ToListAsync();
                ViewData["PatientId"] = new SelectList(patients, "Id", "Email"); // Display email for selection
            }
            else if (User.IsInRole("Patient"))
            {
                // If the current user is a Patient, they can only create an appointment for themselves
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    return Forbid(); // Ensure that the patient can only create their own appointments
                }

                // Populate Doctors dropdown (Only the patient's primary doctor can be selected)
                var doctors = await _context.Doctors.Where(d => d.Id == patient.PrimaryDoctorId).ToListAsync();
                ViewData["DoctorIds"] = new SelectList(doctors, "Id", "Email");
            }

            // Populate AppointmentStatus enum
            ViewData["AppointmentStatus"] = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>());
            ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>());

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Create(Appointment appointment, int[] DoctorIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isDoctor = User.IsInRole("Doctor");
            var isPatient = User.IsInRole("Patient");

            if (isDoctor)
            {
                // Ensure the doctor can only create an appointment for themselves
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
                if (doctor == null)
                {
                    return Forbid();
                }

                // Ensure that the doctor is assigned to the appointment
                appointment.Doctors = new List<Doctor> { doctor };
            }
            else if (isPatient)
            {
                // Ensure the patient can only create an appointment for themselves
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null || patient.Id != appointment.PatientId)
                {
                    return Forbid();
                }

                // Ensure the primary doctor is assigned
                var primaryDoctor = await _context.Doctors.FindAsync(patient.PrimaryDoctorId);
                if (primaryDoctor != null)
                {
                    appointment.Doctors = new List<Doctor> { primaryDoctor };
                }
            }

            // Automatically generate a random bill amount between 100 and 300
            var random = new Random();
            appointment.BillAmount = (float)Math.Round(random.NextDouble() * (301 - 100) + 100, 2);


            if (!ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // Redirect to the details page of the newly created appointment
                return RedirectToAction(nameof(Details), new { id = appointment.Id });
            }

            // Repopulate dropdowns in case of error
            ViewData["PatientId"] = new SelectList(await _context.Patients.ToListAsync(), "Id", "Email", appointment.PatientId);
            ViewData["DoctorIds"] = new MultiSelectList(await _context.Doctors.ToListAsync(), "Id", "Email", DoctorIds);
            ViewData["AppointmentStatus"] = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>(), appointment.AppointmentStatus);
            ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>(), appointment.FeedbackStatus);

            return View(appointment);
        }



        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Get the doctors already assigned to the appointment
            var assignedDoctors = appointment.Doctors.Select(d => d.Id).ToList();

            // Get the unassigned doctors (those not already assigned to the appointment)
            var unassignedDoctors = await _context.Doctors
                .Where(d => !assignedDoctors.Contains(d.Id))
                .OrderBy(d => d.FirstName).ThenBy(d => d.LastName) // Sort alphabetically
                .ToListAsync();

            // Populate the view data
            ViewData["PatientId"] = new SelectList(await _context.Patients.ToListAsync(), "Id", "Email", appointment.PatientId);

            ViewBag.AssignedDoctors = appointment.Doctors; // Pass assigned doctors to the view
            ViewBag.UnassignedDoctors = unassignedDoctors; // Pass unassigned doctors to the view

            ViewData["AppointmentStatus"] = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>(), appointment.AppointmentStatus);
            ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>(), appointment.FeedbackStatus);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment, int[] DoctorIds)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
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

                    // Update basic appointment details
                    existingAppointment.AppointmentDate = appointment.AppointmentDate;
                    existingAppointment.AppointmentStatus = appointment.AppointmentStatus;
                    existingAppointment.BillAmount = appointment.BillAmount;
                    existingAppointment.FeedbackStatus = appointment.FeedbackStatus;
                    existingAppointment.PatientNotification = appointment.PatientNotification;
                    existingAppointment.DoctorNotification = appointment.DoctorNotification;

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

            // Repopulate dropdowns in case of error
            ViewData["PatientId"] = new SelectList(await _context.Patients.ToListAsync(), "Id", "Email", appointment.PatientId);
            ViewData["DoctorIds"] = new MultiSelectList(await _context.Doctors.ToListAsync(), "Id", "Email", DoctorIds);
            ViewData["AppointmentStatus"] = new SelectList(Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>(), appointment.AppointmentStatus);
            ViewData["FeedbackStatus"] = new SelectList(Enum.GetValues(typeof(FeedbackStatus)).Cast<FeedbackStatus>(), appointment.FeedbackStatus);

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
