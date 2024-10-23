using Hospital_Managment_System.Data;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital_Managment_System.Controllers
{
    [Authorize] // Ensures that only authenticated users can access this controller
    public class BillingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BillingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Generate billing automatically when an appointment is created
        public async Task<IActionResult> CreateBillingForAppointment(int appointmentId, DateTime billingDate)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Randomly select payment method and billing status
            var random = new Random();
            var paymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>().ToList();
            var billingStatus = Enum.GetValues(typeof(BillStatus)).Cast<BillStatus>().ToList();

            // Randomly generate the amount
            decimal billAmount = (decimal)(random.NextDouble() * 500 + 50); // Random amount between 50 and 550

            var newBilling = new Billing
            {
                AppointmentId = appointmentId,
                Amount = billAmount,
                BillingDate = billingDate,
                //PaymentMethod = paymentMethods[random.Next(paymentMethods.Count)].ToString(),
                Status = billingStatus[random.Next(billingStatus.Count)]
            };

            // If status is "Paid", add payment date; if "Unpaid", add due date
            if (newBilling.Status == BillStatus.Paid)
            {
                newBilling.PaymentDate = billingDate.AddDays(random.Next(1, 30)); // Payment made within 1-30 days
            }
            else
            {
                newBilling.DueDate = billingDate.AddDays(random.Next(30, 60)); // Due date is 30-60 days after billing
            }

            _context.Billings.Add(newBilling);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Appointments", new { id = appointmentId });
        }

        // GET: Billings (Admin can see all, Patient can only see their own)
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Billing> billings;

            if (isAdmin)
            {
                // Admin can view all billing records
                billings = _context.Billings.Include(b => b.Appointment).ThenInclude(a => a.Patient);
            }
            else
            {
                // Patients can only view their own billing records
                billings = _context.Billings
                    .Include(b => b.Appointment)
                    .Where(b => b.Appointment.Patient.UserId == currentUser.Id);
            }

            return View(await billings.ToListAsync());
        }

        // GET: Billings/Details/5
        [Authorize(Roles = "Admin,Patient")] // Ensure only Admin and Patient can view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var billing = await _context.Billings
                .Include(b => b.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (billing == null)
            {
                return NotFound();
            }

            // Ensure that the patient can only view their own bills
            if (!isAdmin && billing.Appointment.Patient.UserId != currentUser.Id)
            {
                return Forbid(); // Return forbidden if the patient tries to access another patient's bill
            }

            return View(billing);
        }

        // Edit and Delete No one Can create the Billing Eigther.
    }
}
