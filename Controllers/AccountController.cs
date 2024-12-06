using Hospital_Managment_System.Models;
using Hospital_Managment_System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hospital_Managment_System.Models.ViewModels;
using Hospital_Managment_System.Enums;
using System.Threading.Tasks;

namespace Hospital_Managment_System.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect based on role
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("AdminIndex", "Home");
                }
                else if (roles.Contains("Doctor"))
                {
                    return RedirectToAction("DoctorIndex", "Home");
                }
                else if (roles.Contains("Patient"))
                {
                    return RedirectToAction("PatientIndex", "Home");
                }
                else
                {
                    // Default fallback if no role is found
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure roles exist
                    await EnsureRolesExist();

                    // Assign Patient role
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
                        DateTimeOfAdmission = DateTime.Now,
                        UserId = user.Id
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Add any errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return view with validation errors if registration fails
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Ensures that the necessary roles are created in the system.
        /// </summary>
        /// <returns></returns>
        private async Task EnsureRolesExist()
        {
            foreach (var role in Enum.GetNames(typeof(UserRoles)))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole(role);
                    await _roleManager.CreateAsync(identityRole);
                }
            }
        }
    }
}
