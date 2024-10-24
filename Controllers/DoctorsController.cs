using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Hospital_Managment_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managment_System.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //GET: Doctors : Index
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Index(string searchQuery, string departmentId, string specialization, bool availableForPrimary = false, int page = 1)
        {
            int pageSize = 8; // Define page size for pagination

            // Get all doctors
            var doctorsQuery = _context.Doctors.Include(d => d.Department).AsNoTracking();

            // If there's a search query, filter the doctors by name
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                doctorsQuery = doctorsQuery.Where(d =>
                    (d.FirstName + " " + d.LastName).Contains(searchQuery) ||
                    d.FirstName.Contains(searchQuery) ||
                    d.LastName.Contains(searchQuery));
            }

            // Filter by department if selected
            if (!string.IsNullOrEmpty(departmentId))
            {
                doctorsQuery = doctorsQuery.Where(d => d.DepartmentId == int.Parse(departmentId));
            }

            // Filter by specialization if selected
            if (!string.IsNullOrEmpty(specialization))
            {
                doctorsQuery = doctorsQuery.Where(d => d.Specialization.ToString() == specialization);
            }

            // Filter for doctors available to be primary doctor
            if (availableForPrimary)
            {
                doctorsQuery = doctorsQuery.Where(d => d.Patients.Count < 20);
            }

            // Order doctors alphabetically by first name and paginate results
            var doctorsPaged = await doctorsQuery
                .OrderBy(d => d.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(doctorsQuery.Count() / (double)pageSize);

            // Populate ViewData for dropdowns, sorted alphabetically
            ViewData["Departments"] = new SelectList(
                await _context.Departments.OrderBy(d => d.Name).ToListAsync(),
                "Id", "Name");

            ViewData["Specializations"] = new SelectList(
                Enum.GetValues(typeof(DoctorSpecialization))
                .Cast<DoctorSpecialization>()
                .OrderBy(s => s.ToString())
                .Select(s => new { Value = (int)s, Text = s.ToString() }),
                "Value", "Text");

            return View("PatientLimitedDetails", doctorsPaged);
        }

        // API to provide autocomplete suggestions
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetDoctorSuggestions(string term)
        {
            var suggestions = await _context.Doctors
                .Where(d => d.FirstName.Contains(term) || d.LastName.Contains(term))
                .Select(d => $"{d.FirstName} {d.LastName}")
                .ToListAsync();

            return Json(suggestions);
        }

        // GET: Doctors/Details/5
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Authorization logic for Doctors (excluding Admins)
            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (doctor.UserId != currentUser.Id)
                {
                    var statusOnly = new Doctor
                    {
                        Id = doctor.Id,
                        FirstName = doctor.FirstName,
                        LastName = doctor.LastName,
                        BirthDate = doctor.BirthDate,
                        Specialization = doctor.Specialization,
                        Department = doctor.Department,
                        Status = doctor.Status,
                        Email = doctor.Email,
                        Phone = doctor.Phone ?? "N/A"
                    };
                    return View("StatusOnlyDetails", statusOnly);
                }
            }

            // Logic for patients (or admins) to view and set primary doctor
            if (User.IsInRole("Patient"))
            {
                // Get current patient by user ID
                var currentUser = await _userManager.GetUserAsync(User);
                var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

                if (currentPatient != null)
                {
                    bool hasPrimaryDoctor = currentPatient.PrimaryDoctorId != null;
                    bool isCurrentPrimaryDoctor = currentPatient.PrimaryDoctorId == doctor.Id;

                    ViewBag.HasPrimaryDoctor = hasPrimaryDoctor;
                    ViewBag.IsCurrentPrimaryDoctor = isCurrentPrimaryDoctor;

                    // Return view with limited doctor information for the patient
                    return View("PatientDoctorDetailsView", doctor);
                }

                // If the user is a patient but is not found in the Patients table
                return Unauthorized();
            }

            // Admin can view all details
            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Populate Departments dropdown
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");

            // Populate Gender enum
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>());

            // Populate DoctorSpecialization enum
            ViewData["Specialization"] = new SelectList(Enum.GetValues(typeof(DoctorSpecialization)).Cast<DoctorSpecialization>());

            // Populate DoctorStatus enum
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(DoctorStatus)).Cast<DoctorStatus>());

            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Create a new IdentityUser for the Doctor
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign the Doctor role
                    await _userManager.AddToRoleAsync(user, UserRoles.Doctor.ToString());

                    // Create a corresponding Doctor record
                    var doctor = new Doctor
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        BirthDate = model.BirthDate,
                        Gender = model.Gender,
                        Email = model.Email,
                        Phone = model.Phone,
                        Specialization = model.Specialization,
                        Status = model.Status,
                        DepartmentId = model.DepartmentId,
                        UserId = user.Id
                    };

                    _context.Doctors.Add(doctor);
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
            ViewData["Gender"] = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), model.Gender);
            ViewData["Specialization"] = new SelectList(Enum.GetValues(typeof(DoctorSpecialization)).Cast<DoctorSpecialization>(), model.Specialization);
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(DoctorStatus)).Cast<DoctorStatus>(), model.Status);

            return View(model);
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // If the user is a Doctor, ensure they can only edit their own information
            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (doctor.UserId != currentUser.Id)
                {
                    return Forbid(); // Prevent the doctor from editing someone else's profile
                }
            }

            // Populate the ViewModel
            var model = new DoctorViewModel
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                BirthDate = doctor.BirthDate,
                Gender = doctor.Gender,
                Email = doctor.Email,
                Phone = doctor.Phone,
                Specialization = doctor.Specialization,
                Status = doctor.Status,
                DepartmentId = doctor.DepartmentId,
                IsEditMode = true // Set to true when editing
            };

            // Populate dropdown lists including Department
            PopulateDropdowns(doctor.DepartmentId, doctor.Gender, doctor.Specialization, doctor.Status);

            return View(model);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, DoctorViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // If the user is a Doctor, ensure they can only edit their own information
            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var existingDoctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                if (existingDoctor == null || existingDoctor.UserId != currentUser.Id)
                {
                    return Forbid(); // Prevent unauthorized modification
                }
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingDoctor = await _context.Doctors.FindAsync(id);
                    if (existingDoctor == null)
                    {
                        return NotFound();
                    }

                    // Update the doctor's basic info
                    existingDoctor.FirstName = model.FirstName;
                    existingDoctor.LastName = model.LastName;
                    existingDoctor.BirthDate = model.BirthDate;
                    existingDoctor.Gender = model.Gender;
                    existingDoctor.Email = model.Email;
                    existingDoctor.Phone = model.Phone;
                    existingDoctor.Specialization = model.Specialization;
                    existingDoctor.Status = model.Status;
                    existingDoctor.DepartmentId = model.DepartmentId;

                    // Update the email in IdentityUser if it has changed
                    var user = await _userManager.FindByIdAsync(existingDoctor.UserId);
                    if (user != null && user.Email != model.Email)
                    {
                        user.Email = model.Email;
                        user.UserName = model.Email;
                        var result = await _userManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }

                            // Repopulate dropdown lists and return the view
                            PopulateDropdowns(model.DepartmentId, model.Gender, model.Specialization, model.Status);
                            return View(model);
                        }
                    }

                    _context.Update(existingDoctor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Repopulate dropdown lists in case of validation errors
            PopulateDropdowns(model.DepartmentId, model.Gender, model.Specialization, model.Status);
            return View(model);
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }

        // Method to populate the dropdown lists including Department
        private void PopulateDropdowns(int departmentId, Gender gender, DoctorSpecialization specialization, DoctorStatus status)
        {
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", departmentId);
            ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>(), gender);
            ViewBag.Specialization = new SelectList(Enum.GetValues(typeof(DoctorSpecialization)).Cast<DoctorSpecialization>(), specialization);
            ViewBag.Status = new SelectList(Enum.GetValues(typeof(DoctorStatus)).Cast<DoctorStatus>(), status);
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                // Delete associated IdentityUser
                var user = await _userManager.FindByIdAsync(doctor.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View("Details", doctor);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SetPrimaryDoctor(int doctorId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null || patient.PrimaryDoctorId != null)
            {
                return BadRequest("You already have a primary doctor or an error occurred.");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && d.Patients.Count < 20);
            if (doctor == null)
            {
                return NotFound("Doctor not available.");
            }

            patient.PrimaryDoctorId = doctorId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Doctors");
        }



    }
}
