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

        // GET: LabTests/ViewTestResults/5 (Both Doctor and Patient can view specific test results)
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> ViewTestResults(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var labTest = await _context.LabTests
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Doctors)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (labTest == null)
            {
                return NotFound();
            }

            // Patients can only view their own test results
            if (User.IsInRole("Patient"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

                if (labTest.Appointment.PatientId != patient.Id)
                {
                    return Forbid();
                }
            }

            return View(labTest);
        }


        public async Task<IActionResult> ViewLabReport(int appointmentId)
        {
            var labTests = await _context.LabTests
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .Where(l => l.AppointmentId == appointmentId)
                .ToListAsync();

            if (labTests == null || labTests.Count == 0)
            {
                return NotFound();
            }

            return View("LabReport", labTests);
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
                var labTest = new LabTest
                {
                    AppointmentId = appointmentId,
                    TestName = Enum.Parse<LabTestName>(test),
                    TestDate = DateTime.Now,  // Set TestDate to the current date and time
                    IsCompleted = false  // Set the test to incomplete by default
                };

                _context.LabTests.Add(labTest);
            }

            await _context.SaveChangesAsync();

            // Automatically generate the lab report after saving the lab tests
            await GenerateAndSaveLabReport(appointmentId);

            // Redirect back to the Edit page after generating the report
            return RedirectToAction("Edit", "Appointments", new { id = appointmentId });
        }


       


        // Method to automatically generate the lab report and save it
        private async Task GenerateAndSaveLabReport(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctors)
                .Include(a => a.LabTests)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return;
            }

            //// Generate the lab report only if there are completed tests
            //if (!appointment.LabTests.Any(lt => lt.IsCompleted))
            //{
            //    return;
            //}

            // Generate the report content
            var reportHtml = GenerateLabReportHtml(appointment);

            // Save the report in the database if it hasn't been saved already
            if (appointment.LabReport == null)
            {
                appointment.LabReport = reportHtml;
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
        }

        // Method to generate the HTML content for the lab report
        private string GenerateLabReportHtml(Appointment appointment)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head><style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; }");
            sb.AppendLine("h1 { text-align: center; color: darkblue; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { padding: 10px; border: 1px solid #ddd; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine("</style></head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<h1>Lab Report</h1>");
            sb.AppendLine("<p><strong>Patient Name:</strong> " + appointment.Patient.FirstName + " " + appointment.Patient.LastName + "</p>");
            sb.AppendLine("<p><strong>Doctor:</strong> " + appointment.Doctors.FirstOrDefault()?.FirstName + " " + appointment.Doctors.FirstOrDefault()?.LastName + "</p>");
            sb.AppendLine("<p><strong>Report Date:</strong> " + DateTime.Now.ToString("MMMM dd, yyyy") + "</p>");
            sb.AppendLine("<hr />");

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Test Name</th><th>Result</th><th>Units</th><th>Reference Range</th><th>Flag</th></tr>");

            foreach (var test in appointment.LabTests.Where(t => t.IsCompleted))
            {
                var result = test.TestResult;
                var flag = LabTestHelper.GetFlagForTestResult(test);
                sb.AppendLine($"<tr><td>{test.TestName}</td><td>{result}</td><td>{LabTestHelper.GetUnitsForTest(test.TestName)}</td><td>{LabTestHelper.GetReferenceRangeForTest(test.TestName)}</td><td>{flag}</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }


        private bool LabTestExists(int id)
        {
            return _context.LabTests.Any(e => e.Id == id);
        }

        private string GetUnitsForTest(LabTestName testName)
        {
            switch (testName)
            {
                case LabTestName.CompleteBloodCount: return "x10^9/L";
                case LabTestName.BasicMetabolicPanel: return "mg/dL";
                case LabTestName.ComprehensiveMetabolicPanel: return "mg/dL";
                case LabTestName.LipidPanel: return "mg/dL";
                case LabTestName.LiverFunctionTest: return "U/L";
                case LabTestName.ThyroidFunctionTest: return "mU/L";
                case LabTestName.HemoglobinA1c: return "%";
                case LabTestName.Urinalysis: return "pH, Specific Gravity";
                case LabTestName.BloodGlucose: return "mg/dL";
                case LabTestName.ProthrombinTime: return "seconds";
                case LabTestName.DDimers: return "mg/L";
                case LabTestName.CReactiveProtein: return "mg/L";
                case LabTestName.RheumatoidFactor: return "IU/mL";
                case LabTestName.ErythrocyteSedimentationRate: return "mm/hr";
                case LabTestName.IronStudies: return "mcg/dL";
                case LabTestName.VitaminDTest: return "ng/mL";
                case LabTestName.VitaminB12Test: return "pg/mL";
                case LabTestName.ElectrolytePanel: return "mEq/L";
                case LabTestName.ArterialBloodGas: return "mmHg";
                case LabTestName.BloodUreaNitrogen: return "mg/dL";
                default: return "Units";
            }
        }

        private string GetReferenceRangeForTest(LabTestName testName)
        {
            switch (testName)
            {
                case LabTestName.CompleteBloodCount: return "4.0 - 11.0 x10^9/L";
                case LabTestName.BasicMetabolicPanel: return "70 - 100 mg/dL";
                case LabTestName.ComprehensiveMetabolicPanel: return "8.5 - 10.2 mg/dL";
                case LabTestName.LipidPanel: return "130 - 200 mg/dL (Total Cholesterol)";
                case LabTestName.LiverFunctionTest: return "10 - 40 U/L";
                case LabTestName.ThyroidFunctionTest: return "0.4 - 4.0 mU/L";
                case LabTestName.HemoglobinA1c: return "<5.7%";
                case LabTestName.Urinalysis: return "pH: 4.5 - 8.0, SG: 1.005 - 1.030";
                case LabTestName.BloodGlucose: return "70 - 99 mg/dL";
                case LabTestName.ProthrombinTime: return "11 - 13.5 seconds";
                case LabTestName.DDimers: return "<0.50 mg/L";
                case LabTestName.CReactiveProtein: return "<10 mg/L";
                case LabTestName.RheumatoidFactor: return "<20 IU/mL";
                case LabTestName.ErythrocyteSedimentationRate: return "0 - 20 mm/hr";
                case LabTestName.IronStudies: return "60 - 170 mcg/dL";
                case LabTestName.VitaminDTest: return "30 - 100 ng/mL";
                case LabTestName.VitaminB12Test: return "200 - 900 pg/mL";
                case LabTestName.ElectrolytePanel: return "Sodium: 135 - 145 mEq/L, Potassium: 3.5 - 5.0 mEq/L";
                case LabTestName.ArterialBloodGas: return "pH: 7.35 - 7.45, pCO2: 35 - 45 mmHg";
                case LabTestName.BloodUreaNitrogen: return "7 - 20 mg/dL";
                default: return "N/A";
            }
        }

        private string GetFlagForTestResult(LabTest test)
        {
            double result;

            // Attempt to parse the result as a double (you may need to handle other types depending on your results)
            if (!double.TryParse(test.TestResult, out result))
            {
                return "N/A";  // If the result can't be parsed, return N/A
            }

            switch (test.TestName)
            {
                case LabTestName.CompleteBloodCount:
                    if (result < 4.0) return "LOW";
                    if (result > 11.0) return "HIGH";
                    return "NORMAL";

                case LabTestName.BasicMetabolicPanel:
                    if (result < 70) return "LOW";
                    if (result > 100) return "HIGH";
                    return "NORMAL";

                case LabTestName.ComprehensiveMetabolicPanel:
                    if (result < 8.5) return "LOW";
                    if (result > 10.2) return "HIGH";
                    return "NORMAL";

                case LabTestName.LipidPanel:
                    if (result < 130) return "LOW";
                    if (result > 200) return "HIGH";
                    return "NORMAL";

                case LabTestName.LiverFunctionTest:
                    if (result < 10) return "LOW";
                    if (result > 40) return "HIGH";
                    return "NORMAL";

                case LabTestName.ThyroidFunctionTest:
                    if (result < 0.4) return "LOW";
                    if (result > 4.0) return "HIGH";
                    return "NORMAL";

                case LabTestName.HemoglobinA1c:
                    if (result >= 5.7) return "HIGH";
                    return "NORMAL";

                case LabTestName.BloodGlucose:
                    if (result < 70) return "LOW";
                    if (result > 99) return "HIGH";
                    return "NORMAL";

                case LabTestName.ProthrombinTime:
                    if (result < 11) return "LOW";
                    if (result > 13.5) return "HIGH";
                    return "NORMAL";

                case LabTestName.DDimers:
                    if (result >= 0.5) return "HIGH";
                    return "NORMAL";

                case LabTestName.CReactiveProtein:
                    if (result >= 10) return "HIGH";
                    return "NORMAL";

                case LabTestName.RheumatoidFactor:
                    if (result >= 20) return "HIGH";
                    return "NORMAL";

                case LabTestName.ErythrocyteSedimentationRate:
                    if (result > 20) return "HIGH";
                    return "NORMAL";

                case LabTestName.IronStudies:
                    if (result < 60) return "LOW";
                    if (result > 170) return "HIGH";
                    return "NORMAL";

                case LabTestName.VitaminDTest:
                    if (result < 30) return "LOW";
                    if (result > 100) return "HIGH";
                    return "NORMAL";

                case LabTestName.VitaminB12Test:
                    if (result < 200) return "LOW";
                    if (result > 900) return "HIGH";
                    return "NORMAL";

                case LabTestName.ElectrolytePanel:
                    if (result < 135) return "LOW";
                    if (result > 145) return "HIGH";
                    return "NORMAL";

                case LabTestName.BloodUreaNitrogen:
                    if (result < 7) return "LOW";
                    if (result > 20) return "HIGH";
                    return "NORMAL";

                // Add cases for the rest of the LabTestNames as needed
                default:
                    return "N/A";  // For any undefined or unknown tests, return N/A
            }
        }


    }
}
