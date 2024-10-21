using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // GET: Doctor
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();

            // Create a dictionary of Doctor IDs to Department Names
            var doctorDepartmentNames = doctors.ToDictionary(
                doc => doc.Id,
                doc => doc.Department?.Name ?? "No Department"
            );

            ViewBag.DoctorDepartmentNames = doctorDepartmentNames;

            return View(doctors);
        }

        // GET: Doctor/Details/5
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

            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (doctor.UserId != currentUser.Id)
                {
                    // Doctors can view all Doctors' status but not their details
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
                        Phone = doctor.Phone ?? " "  
                    };
                    //return View("StatusOnlyDetails", statusOnly);
                    return View("StatusOnlyDetails",statusOnly);
                }
            }

            if (User.IsInRole("Patient") && !User.IsInRole("Admin"))
            {
                // Patients can view only limited information
                var limitedInfo = new Doctor
                {
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Specialization = doctor.Specialization,
                    Department = doctor.Department,
                    Email = " ", // Required member
                    Phone = " "  // Required member
                };
                return View("PatientLimitedDetails", limitedInfo);
            }

            return View(doctor);
        }

        // GET: Doctor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            // Create Doctor entity
            var doctor = new Doctor
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                BirthDate = DateTime.MinValue, // Fixed the type to DateTime
                Gender = Gender.Other,
                Email = string.Empty,
                Phone = string.Empty,
                Specialization = DoctorSpecialization.Cardiologist, // Assuming a default value
                Status = DoctorStatus.Active, // Assuming a default value
                DepartmentId = 0, // Assuming a default value
                UserId = string.Empty,
            };
            return View(doctor); // Ensure a return statement is present
        }


        // POST: Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Gender,Email,Phone,Specialization,Status,DepartmentId")] Doctor doctor)
        {
            if (!ModelState.IsValid)
            {
                // Create a new IdentityUser for the doctor
                var user = new IdentityUser
                {
                    UserName = doctor.Email,
                    Email = doctor.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, "Doctor@123"); // Assign a secure default password or handle appropriately

                if (result.Succeeded)
                {
                    // Assign Doctor role
                    await _userManager.AddToRoleAsync(user, "Doctor");

                    // Associate the doctor with the user
                    doctor.UserId = user.Id;
                    doctor.User = user;

                    _context.Add(doctor);
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
            ViewBag.Departments = _context.Departments.ToList();
            return View(doctor);
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
                    return Forbid();
                }
            }

            ViewBag.Departments = _context.Departments.ToList();
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,BirthDate,Gender,Email,Phone,Specialization,Status,DepartmentId")] Doctor doctor)
        {
            if (id != doctor.Id)
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
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingDoctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    if (existingDoctor.Email != doctor.Email)
                    {
                        var user = await _userManager.FindByIdAsync(existingDoctor.UserId);
                        if (user != null)
                        {
                            user.Email = doctor.Email;
                            user.UserName = doctor.Email; // If using Email as Username
                            var result = await _userManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                                ViewBag.Departments = _context.Departments.ToList();
                                return View(doctor);
                            }
                        }
                    }

                    doctor.UserId = existingDoctor.UserId;
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Departments = _context.Departments.ToList();
            return View(doctor);
        }

        // GET: Doctor/Delete/5
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

            ViewBag.DepartmentName = doctor.Department?.Name; // Pass the department name to the view
            return View(doctor);
        }

        // POST: Doctor/Delete/5
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

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }

    }
}
