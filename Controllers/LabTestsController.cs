using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Helpers;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managment_System.Controllers
{
    [Authorize(Roles = "Doctor, Patient")]
    public class LabTestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LabTestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LabTests/Index
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return Forbid();
            }

            // Get all patients for whom this doctor ordered lab tests
            var patientsWithLabTests = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Doctors.Any(d => d.Id == doctor.Id) && a.LabTests.Any())
                .Select(a => a.Patient)
                .Distinct()
                .ToListAsync();

            return View(patientsWithLabTests);
        }

        // GET: LabTests/ViewOrderedTests/5 (Doctor viewing a specific patient's ordered tests)
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ViewOrderedTests(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return Forbid();
            }

            var appointment = await _context.Appointments
                .Include(a => a.LabTests)
                .ThenInclude(l => l.Appointment)
                .Include(a => a.Patient)
                .Where(a => a.PatientId == id && a.Doctors.Any(d => d.Id == doctor.Id))
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment.LabTests);
        }

        // GET: LabTests/PatientTests
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PatientTests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return Forbid();
            }

            // Get all lab tests for the current patient
            var patientLabTests = await _context.Appointments
                .Include(a => a.LabTests)
                .Where(a => a.PatientId == patient.Id)
                .SelectMany(a => a.LabTests)
                .ToListAsync();

            return View(patientLabTests);
        }

        // GET: LabTests/ViewTestResults/5
        [HttpGet]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> ViewTestResults(int? id)
        {
            if (id == null)
            {
                return BadRequest("Test ID is required.");
            }

            // Retrieve the lab test and include related data such as the appointment, patient, and doctors
            var labTest = await _context.LabTests
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Doctors)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (labTest == null)
            {
                return NotFound("Lab test not found.");
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the current user is a doctor
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
                if (doctor == null || !labTest.Appointment.Doctors.Any(d => d.Id == doctor.Id))
                {
                    return Forbid(); // Doctor can only view their own patients' test results
                }
            }

            // Check if the current user is a patient
            if (User.IsInRole("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null || labTest.Appointment.PatientId != patient.Id)
                {
                    return Forbid(); // Patients can only view their own test results
                }
            }

            // Render the View with the lab test data
            return View(labTest);
        }


        // GET: LabTests/OrderLabTests
        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public IActionResult OrderLabTests(int appointmentId)
        {
            // Fetch the appointment or other necessary data
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Pass the appointmentId to the view so it can be used for ordering tests
            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        // POST: LabTests/OrderMultipleTests
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderMultipleTests(int appointmentId, string[] selectedTests)
        {
            if (selectedTests == null || selectedTests.Length == 0)
            {
                return BadRequest("No tests were selected.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                return Forbid();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctors)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Doctors.Any(d => d.Id == doctor.Id));

            if (appointment == null)
            {
                return NotFound();
            }

            foreach (var test in selectedTests)
            {
                var testName = Enum.Parse<LabTestName>(test);
                var labTest = new LabTest
                {
                    AppointmentId = appointmentId,
                    TestName = testName,
                    TestDate = DateTime.Now,  // Set TestDate to the current date and time
                    IsCompleted = true,  // Set to true since we're generating the result right away
                    TestResult = GenerateFormattedLabTestResult(testName) // Generate formatted result string
                };

                _context.LabTests.Add(labTest);
            }

            await _context.SaveChangesAsync();

            // Redirect back to the Edit page after generating the report
            return RedirectToAction("Edit", "Appointments", new { id = appointmentId });
        }

        // Generate the full formatted lab test result string using plain text formatting
        private string GenerateFormattedLabTestResult(LabTestName testName)
        {
            var resultValue = GenerateRandomTestResult(testName);
            var units = LabTestHelper.GetUnitsForTest(testName);
            var referenceRange = LabTestHelper.GetReferenceRangeForTest(testName);
            var flag = LabTestHelper.GetFlagForTestResult(new LabTest { TestName = testName, TestResult = resultValue });

            // Format the string with plain text using \n for line breaks
            var resultString = $"Test Name: {testName}\n" +
                               $"Result: {resultValue} {units}\n" +
                               $"Reference Range: {referenceRange}\n" +
                               $"Flag: {flag}\n";

            return resultString;
        }

        // Generate random test results for each lab test within valid ranges
        private string GenerateRandomTestResult(LabTestName testName)
        {
            var random = new Random();
            double result;

            // Generate random results based on the test type and valid range
            switch (testName)
            {
                case LabTestName.CompleteBloodCount:
                    result = Math.Round(random.NextDouble() * (11.0 - 4.0) + 4.0, 2);
                    break;

                case LabTestName.BasicMetabolicPanel:
                    result = Math.Round(random.NextDouble() * (100 - 70) + 70, 2);
                    break;

                case LabTestName.ComprehensiveMetabolicPanel:
                    result = Math.Round(random.NextDouble() * (10.2 - 8.5) + 8.5, 2);
                    break;

                case LabTestName.LipidPanel:
                    result = Math.Round(random.NextDouble() * (200 - 130) + 130, 2);
                    break;

                case LabTestName.LiverFunctionTest:
                    result = Math.Round(random.NextDouble() * (40 - 10) + 10, 2);
                    break;

                case LabTestName.ThyroidFunctionTest:
                    result = Math.Round(random.NextDouble() * (4.0 - 0.4) + 0.4, 2);
                    break;

                case LabTestName.HemoglobinA1c:
                    result = Math.Round(random.NextDouble() * (6.5 - 4.0) + 4.0, 2);
                    break;

                case LabTestName.BloodGlucose:
                    result = Math.Round(random.NextDouble() * (99 - 70) + 70, 2);
                    break;

                case LabTestName.ProthrombinTime:
                    result = Math.Round(random.NextDouble() * (13.5 - 11.0) + 11.0, 2);
                    break;

                case LabTestName.DDimers:
                    result = Math.Round(random.NextDouble() * (0.5 - 0.0), 2);
                    break;

                case LabTestName.CReactiveProtein:
                    result = Math.Round(random.NextDouble() * (10 - 0), 2);
                    break;

                case LabTestName.ElectrolytePanel:
                    result = Math.Round(random.NextDouble() * (145 - 135) + 135, 2); // Example for Sodium levels
                    break;

                case LabTestName.BloodUreaNitrogen:
                    result = Math.Round(random.NextDouble() * (20 - 7) + 7, 2);
                    break;

                default:
                    result = Math.Round(random.NextDouble() * 100, 2);  // Default random value
                    break;
            }

            return result.ToString();
        }

    }
}
