using Hospital_Managment_System.Data;
using Hospital_Managment_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hospital_Managment_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            ViewBag.DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}";
            return View();
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> patientIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminIndex()
        {
            return View();
        }
    }
}
