using Hospital_Managment_System.Data;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managment_System.Controllers
{
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
            var patients = await _context.Patients
                .Include(p => p.User)
                .ToListAsync();

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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            // For patients to Only View their own details
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
            // Pass a new instance of Patient to the view
            var patient = new Patient
            {
                FirstName = string.Empty, // Set default value
                LastName = string.Empty, // Set default value
                PhoneNumber = string.Empty, // Set default value
                Email = string.Empty, // Set default value
                Address = string.Empty, // Set default value
                EmergencyContact = string.Empty, // Set default value
                DateTimeOfAdmission = DateTime.Now // Set default DateTimeOfAdmission
            };
            return View(patient);
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth,Gender,PhoneNumber,Email,Address,EmergencyContact,DateTimeOfAdmission")] Patient patient)
        {
            
            if (!ModelState.IsValid)
            {
                // Create new User Identity for the Patient
                var user = new IdentityUser
                {
                    UserName = patient.Email,
                    Email = patient.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Patient@123"); // Assign a default password or handle appropriately

                if (result.Succeeded)
                {
                    // Assign the user to the Patient Role
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Associate the patient with the user
                    patient.UserId = user.Id;
                    patient.User = user; // Ensure the User property is set

                    // Ensure the patient instance is added to the context
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(patient);
        }



        // GET: Patients/Edit/5
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
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

            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,PhoneNumber,Email,Address,EmergencyContact,DateTimeOfAdmission,UserId")] Patient patient)
        {
            if (id != patient.Id)
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

            if (!ModelState.IsValid)
            {
                try
                {
                    // Update the IdentityUser if Email has changed
                    var user = await _userManager.FindByIdAsync(patient.UserId);
                    if (user != null)
                    {
                        if (user.Email != patient.Email)
                        {
                            user.Email = patient.Email;
                            user.UserName = patient.Email; // If using Email as Username
                            var result = await _userManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                // Handle errors
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                                return View(patient);
                            }
                        }
                    }

                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (!(User.IsInRole("Patient")))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(MyDetails));
                }

            }
            return View(patient);
        }


        private bool PatientExists(int id)
        {
            return _context.Patients.Any(_context => _context.Id == id);
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }


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

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return NotFound();
            }

            return View("Details", patient);
        }

    }
}
