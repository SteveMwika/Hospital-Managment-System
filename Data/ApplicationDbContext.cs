using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managment_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // Ensures proper initialization
        {
        }

        // DbSet properties
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<MedicineInventoryLog> MedicineInventoryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Department Entity
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // Configure Medicine Entity
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(m => m.Description)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(m => m.Price)
                      .HasColumnType("decimal(18,2)");
                entity.Property(m => m.Quantity)
                      .IsRequired();

                entity.HasMany(m => m.Prescriptions)
                      .WithOne(p => p.Medicine)
                      .HasForeignKey(p => p.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.InventoryLogs)
                      .WithOne(il => il.Medicine)
                      .HasForeignKey(il => il.MedicineId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Doctor Entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(d => d.LastName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(d => d.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(d => d.Phone)
                      .IsRequired()
                      .HasMaxLength(15);
                entity.Property(d => d.BirthDate)
                      .IsRequired();

                // Enum Conversions
                entity.Property(d => d.Gender)
                      .IsRequired()
                      .HasConversion<int>();
                entity.Property(d => d.Specialization)
                      .IsRequired()
                      .HasConversion<int>();
                entity.Property(d => d.Status)
                      .IsRequired()
                      .HasConversion<int>();

                // Relationships with Department
                entity.HasOne(d => d.Department)
                      .WithMany(dep => dep.Doctors)
                      .HasForeignKey(d => d.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship with Patients (Primary Doctor)
                entity.HasMany(d => d.Patients)
                      .WithOne(p => p.PrimaryDoctor)
                      .HasForeignKey(p => p.PrimaryDoctorId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Many-to-Many with Appointments using skip navigation
                entity.HasMany(d => d.Appointments)
                      .WithMany(a => a.Doctors)
                      .UsingEntity<Dictionary<string, object>>(
                          "AppointmentDoctor",
                          ad => ad.HasOne<Appointment>().WithMany().HasForeignKey("AppointmentId").OnDelete(DeleteBehavior.Cascade),
                          ad => ad.HasOne<Doctor>().WithMany().HasForeignKey("DoctorId").OnDelete(DeleteBehavior.Cascade),
                          ad =>
                          {
                              ad.HasKey("DoctorId", "AppointmentId");
                              ad.ToTable("AppointmentDoctor");
                          });

                // Relationship with IdentityUser
                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Patient Entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(p => p.LastName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(p => p.PhoneNumber)
                      .IsRequired()
                      .HasMaxLength(15);
                entity.Property(p => p.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(p => p.Address)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(p => p.EmergencyContact)
                      .IsRequired()
                      .HasMaxLength(15);

                // Enum Conversion
                entity.Property(p => p.Gender)
                      .HasConversion<int>();

                // Relationships with Appointments
                entity.HasMany(p => p.Appointments)
                      .WithOne(a => a.Patient)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship with IdentityUser
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship with PrimaryDoctor
                entity.HasOne(p => p.PrimaryDoctor)
                      .WithMany(d => d.Patients)
                      .HasForeignKey(p => p.PrimaryDoctorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Appointment Entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AppointmentDate)
                      .IsRequired();
                entity.Property(a => a.AppointmentStatus)
                      .IsRequired()
                      .HasConversion<int>();
                entity.Property(a => a.BillAmount)
                      .HasColumnType("float");
                
                entity.Property(a => a.Feedback)
                      .HasMaxLength(200);
                entity.Property(a => a.Feedback)
                      .HasMaxLength(200)
                      .IsRequired(false); // This makes the field optional (allows nulls)
                      

                // Removed BillStatus string property
                entity.Property(a => a.DoctorNotification)
                      .IsRequired();
                entity.Property(a => a.PatientNotification)
                      .IsRequired();
                entity.Property(a => a.FeedbackStatus)
                      .HasConversion<int>();
                //entity.Property(a => a.BillStatus)
                //      .HasConversion<int>();

                // Relationship with Patient
                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship with Billing
                entity.HasOne(a => a.Billing)
                      .WithOne(b => b.Appointment)
                      .HasForeignKey<Billing>(b => b.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationships with Prescriptions and LabTests
                entity.HasMany(a => a.Prescriptions)
                      .WithOne(p => p.Appointment)
                      .HasForeignKey(p => p.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(a => a.LabTests)
                      .WithOne(l => l.Appointment)
                      .HasForeignKey(l => l.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Concurrency Token
                // Removed per user instruction
            });

            // Configure Billing Entity
            modelBuilder.Entity<Billing>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Amount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(b => b.Status)
                      .IsRequired()
                      .HasConversion<int>(); // Enum stored as int
                entity.Property(b => b.PaymentMethod)
                      .IsRequired()
                      .HasConversion<int>(); // Enum stored as int
                entity.Property(b => b.BillingDate)
                      .IsRequired();
                entity.Property(b => b.PaymentDate)
                      .HasColumnType("Date");
                entity.Property(b => b.DueDate)
                      .HasColumnType("Date");

                // Relationship with Appointment is configured in Appointment
            });

            // Configure Prescription Entity
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Instructions)
                      .IsRequired()
                      .HasMaxLength(200);

                // Relationships
                entity.HasOne(p => p.Appointment)
                      .WithMany(a => a.Prescriptions)
                      .HasForeignKey(p => p.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Medicine)
                      .WithMany(m => m.Prescriptions)
                      .HasForeignKey(p => p.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Doctor)
                      .WithMany()
                      .HasForeignKey(p => p.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Patient)
                      .WithMany()
                      .HasForeignKey(p => p.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LabTest Entity
            // Configure LabTest Entity
            modelBuilder.Entity<LabTest>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.TestName)
                      .IsRequired()
                      .HasConversion<int>();  // Enum is stored as an integer
                entity.Property(l => l.TestResult)
                      .HasMaxLength(500);  // Limit the result text size
                entity.Property(l => l.TestDate)
                      .IsRequired();  // Date when the test was conducted
                entity.Property(l => l.IsCompleted)
                      .IsRequired();  // Ensure the test completion is marked

                entity.HasOne(l => l.Appointment)
                      .WithMany(a => a.LabTests)
                      .HasForeignKey(l => l.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);  // Cascade delete with appointment
            });

            // Configure MedicineInventoryLog Entity
            modelBuilder.Entity<MedicineInventoryLog>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.ChangeType)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(m => m.QuantityChanged)
                      .IsRequired();

                entity.Property(m => m.NewQuantity)
                      .IsRequired();

                entity.Property(m => m.DateTime)
                      .IsRequired();

                entity.Property(m => m.Description)
                      .HasMaxLength(500);

                // Relationships
                entity.HasOne(m => m.Medicine)
                      .WithMany(med => med.InventoryLogs)
                      .HasForeignKey(m => m.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Doctor)
                      .WithMany()
                      .HasForeignKey(m => m.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Patient)
                      .WithMany()
                      .HasForeignKey(m => m.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Prescription)
                      .WithMany()
                      .HasForeignKey(m => m.PrescriptionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.AdminUser)
                      .WithMany()
                      .HasForeignKey(m => m.AdminUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
