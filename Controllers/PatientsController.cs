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
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Patients
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Initialize a query for patients
            IQueryable<Patient> patientsQuery = _context.Patients
                .Include(p => p.User)
                .Include(p => p.PrimaryDoctor)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctors);

            if (User.IsInRole("Doctor"))
            {
                // Get the Doctor associated with the logged-in user
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor != null)
                {
                    // Filter the query to only show patients of the logged-in doctor or associated via an appointment
                    patientsQuery = patientsQuery.Where(p => p.PrimaryDoctorId == doctor.Id ||
                                                             p.Appointments.Any(a => a.Doctors.Any(d => d.Id == doctor.Id)));
                }
            }

            // Execute the query and return the list of patients
            var patients = await patientsQuery.AsNoTracking().ToListAsync();

            return View(patients); // Ensure this returns the correct view for patients
        }

        // GET: Patients/Details/5
        [Authorize(Roles = "Admin, Doctor, Patient")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.PrimaryDoctor)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            // For patients to only view their own details
            if (User.IsInRole("Patient"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || patient.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Populate Departments dropdown
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");

            // Populate Doctors dropdown
            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName");

            // Populate Gender enum
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>());

            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Create a new IdentityUser for the Patient
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign the Patient role
                    await _userManager.AddToRoleAsync(user, UserRoles.Patient.ToString());

                    // Create a corresponding Patient record
                    var patient = new Patient
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Address = model.Address,
                        EmergencyContact = model.EmergencyContact,
                        DateTimeOfAdmission = model.DateTimeOfAdmission ?? DateTime.Now,
                        PrimaryDoctorId = model.PrimaryDoctorId,
                        UserId = user.Id
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we reach here, something failed; repopulate dropdowns
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", model.PrimaryDoctorId);
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), model.Gender);

            return View(model);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.PrimaryDoctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            // Patients can only edit their own details
            if (User.IsInRole("Patient"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || patient.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            // Populate Departments dropdown
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", patient.PrimaryDoctor?.DepartmentId);

            // Populate Doctors dropdown
            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", patient.PrimaryDoctorId);

            // Populate Gender enum, handle null gender
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), patient.Gender ?? Gender.Male); // Default to Male if null

            // Initialize ViewModel
            var model = new PatientViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth ?? default(DateTime),
                Gender = patient.Gender ?? Gender.Male,  // Default if gender is null
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                EmergencyContact = patient.EmergencyContact,
                DateTimeOfAdmission = patient.DateTimeOfAdmission,
                PrimaryDoctorId = patient.PrimaryDoctorId
            };

            return View(model);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> Edit(int id, PatientViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Patients can only edit their own details
            if (User.IsInRole("Patient"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var patient = await _context.Patients.FindAsync(id);
                if (currentUser == null || patient.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var patient = await _context.Patients.FindAsync(id);
                    if (patient == null)
                    {
                        return NotFound();
                    }

                    // Update fields
                    patient.FirstName = model.FirstName;
                    patient.LastName = model.LastName;
                    patient.DateOfBirth = model.DateOfBirth;
                    patient.Gender = model.Gender;
                    patient.PhoneNumber = model.PhoneNumber;
                    patient.Email = model.Email;
                    patient.Address = model.Address;
                    patient.EmergencyContact = model.EmergencyContact;
                    patient.DateTimeOfAdmission = model.DateTimeOfAdmission ?? patient.DateTimeOfAdmission;
                    patient.PrimaryDoctorId = model.PrimaryDoctorId;

                    // Update the IdentityUser email if it has changed
                    var user = await _userManager.FindByIdAsync(patient.UserId);
                    if (user != null && user.Email != model.Email)
                    {
                        user.Email = model.Email;
                        user.UserName = model.Email;
                        var result = await _userManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            // Repopulate dropdowns
                            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
                            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", model.PrimaryDoctorId);
                            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), model.Gender);
                            return View(model);
                        }
                    }

                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirect based on role
                if (User.IsInRole("Patient"))
                {
                    return RedirectToAction(nameof(MyDetails));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we reach here, something failed; repopulate dropdowns
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", model.PrimaryDoctorId);
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), model.Gender);

            return View(model);
        }

        private void PopulateDropdowns(int? departmentId, int? primaryDoctorId, Gender? gender)
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", departmentId);
            ViewData["PrimaryDoctorId"] = new SelectList(_context.Doctors, "Id", "FirstName", primaryDoctorId);
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), gender);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(p => p.Id == id);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.PrimaryDoctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                // Remove the Patient first
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();

                // Then delete the Associated User
                var user = await _userManager.FindByIdAsync(patient.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyDetails()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Fetch patient details including the Primary Doctor and Department (if applicable)
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.PrimaryDoctor)
                .ThenInclude(d => d.Department)  // Ensure the Department is included if the doctor has one
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return NotFound();
            }

            // If the patient does not have a PrimaryDoctor, handle this gracefully in the view
            return View("PatientDetailsMore", patient);
        }
    }
}
