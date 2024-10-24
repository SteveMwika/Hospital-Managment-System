using Hospital_Managment_System.Data;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Doctor, Patient")]
public class PrescriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PrescriptionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Prescription/PatientPrescriptions (For Patient)
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> PatientPrescriptions()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

        if (patient == null)
        {
            return Forbid();
        }

        var prescriptions = await _context.Prescriptions
            .Include(p => p.Medicine)
            .Where(p => p.PatientId == patient.Id)
            .ToListAsync();

        return View(prescriptions);
    }

    // GET: Prescription/DoctorPatients (For Doctor)
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> DoctorPatients()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

        if (doctor == null)
        {
            return Forbid();
        }

        var patientsWithPrescriptions = await _context.Prescriptions
            .Include(p => p.Patient)
            .Where(p => p.DoctorId == doctor.Id)
            .Select(p => p.Patient)
            .Distinct()
            .ToListAsync();

        return View(patientsWithPrescriptions);
    }

    // GET: Prescription/PatientPrescriptions/5 (For Doctor to View a Specific Patient's Prescriptions)
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> ViewPatientPrescriptions(int? id)
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

        // Retrieve patient and prescriptions
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient == null)
        {
            return NotFound("Patient not found.");
        }

        var prescriptions = await _context.Prescriptions
            .Include(p => p.Medicine)
            .Where(p => p.PatientId == id && p.DoctorId == doctor.Id)
            .ToListAsync();

        // Pass both prescriptions and patient name to the view
        ViewBag.PatientFullName = $"{patient.FirstName} {patient.LastName}";
        return View(prescriptions);
    }



    // GET: Prescriptions/AssignPrescription/5
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> AssignPrescription(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return NotFound();
        }

        var medicines = await _context.Medicines.ToListAsync();
        ViewBag.Medicines = medicines;
        ViewBag.AppointmentId = appointmentId;

        return View();
    }

    // POST: Prescriptions/AssignPrescription
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignPrescription(int appointmentId, int medicineId, string instructions)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctors)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return NotFound();
        }

        var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId);
        if (medicine == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == _userManager.GetUserId(User));

        if (doctor == null)
        {
            return NotFound();
        }

        // Create the prescription
        var prescription = new Prescription
        {
            AppointmentId = appointmentId,
            MedicineId = medicineId,
            Instructions = instructions,
            DoctorId = doctor.Id,
            PatientId = appointment.PatientId
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Appointments", new { id = appointmentId });
    }
}
