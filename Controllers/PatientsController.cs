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
        public async Task<IActionResult> Index(string filter)
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
                    if (filter == "PrimaryPatients")
                    {
                        // Filter for Primary Patients only
                        patientsQuery = patientsQuery.Where(p => p.PrimaryDoctorId == doctor.Id);
                    }
                    else if (filter == "AllPatients")
                    {
                        // Filter for all patients involved with the doctor
                        patientsQuery = patientsQuery.Where(p => p.PrimaryDoctorId == doctor.Id ||
                                                                 p.Appointments.Any(a => a.Doctors.Any(d => d.Id == doctor.Id)));
                    }
                    else
                    {
                        // Default behavior: show all associated patients
                        patientsQuery = patientsQuery.Where(p => p.PrimaryDoctorId == doctor.Id ||
                                                                 p.Appointments.Any(a => a.Doctors.Any(d => d.Id == doctor.Id)));
                    }
                }
            }

            // Execute the query and return the list of patients
            var patients = await patientsQuery.AsNoTracking().ToListAsync();

            return View(patients);
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

            // Admins and Doctors: No restrictions to view patient details
            //if (User.IsInRole("Admin") || User.IsInRole("Doctor"))
            if (User.IsInRole("Admin"))
            {
                return View(patient);
            }

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var patient = new Patient
            {
                DateTimeOfAdmission = DateTime.Now
            };

            // Populate Doctors dropdown with those having less than 20 patients
            var eligibleDoctors = _context.Doctors
                .Where(d => d.Patients.Count(p => p.PrimaryDoctorId == d.Id) < 20)
                .Select(d => new
                {
                    d.Id,
                    FullName = d.FirstName + " " + d.LastName
                }).ToList();

            ViewData["PrimaryDoctorId"] = new SelectList(eligibleDoctors, "Id", "FullName");
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>());

            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Gender,PhoneNumber,Email,Address,EmergencyContact,PrimaryDoctorId")] Patient patient)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    // Validate email uniqueness
                    if (await _userManager.FindByEmailAsync(patient.Email) != null)
                    {
                        ModelState.AddModelError("Email", "An account with this email already exists.");
                        PopulateViewData();
                        return View(patient);
                    }

                    // Validate PrimaryDoctorId
                    var primaryDoctor = await _context.Doctors
                        .Include(d => d.Patients)
                        .FirstOrDefaultAsync(d => d.Id == patient.PrimaryDoctorId);

                    if (primaryDoctor == null || primaryDoctor.Patients.Count(p => p.PrimaryDoctorId == primaryDoctor.Id) >= 20)
                    {
                        ModelState.AddModelError("PrimaryDoctorId", "The selected doctor cannot take more patients or does not exist.");
                        PopulateViewData();
                        return View(patient);
                    }

                    // Create a new user account for the patient
                    var user = new IdentityUser
                    {
                        UserName = patient.Email, // Use email as username
                        Email = patient.Email,
                        PhoneNumber = patient.PhoneNumber,
                        EmailConfirmed = true // Assuming no email verification required
                    };

                    // Set default password
                    var result = await _userManager.CreateAsync(user, "Password123");
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        PopulateViewData();
                        return View(patient);
                    }

                    // Assign "Patient" role to the newly created user
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Link the patient with the created user account
                    patient.UserId = user.Id;
                    patient.DateTimeOfAdmission = DateTime.Now; // Automatically set admission time
                    _context.Add(patient);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Patient created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the patient. Please try again.");
                }
            }

            // Log validation errors
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Validation error in {state.Key}: {error.ErrorMessage}");
                }
            }


            PopulateViewData();
            return View(patient);
        }

        // Helper method to populate dropdowns and enums
        private void PopulateViewData()
        {
            var eligibleDoctors = _context.Doctors
                .Where(d => d.Patients.Count(p => p.PrimaryDoctorId == d.Id) < 20)
                .Select(d => new
                {
                    d.Id,
                    FullName = d.FirstName + " " + d.LastName
                }).ToList();

            ViewData["PrimaryDoctorId"] = new SelectList(eligibleDoctors, "Id", "FullName");
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>());
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
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



        [Authorize(Roles = "Admin, Patient")]
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
