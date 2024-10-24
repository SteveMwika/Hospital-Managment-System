using System;
using System.Collections.Generic;
using System.Linq;
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hospital_Managment_System.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, ILogger logger)
        {
            // Apply any pending migrations
            context.Database.Migrate();
            logger.LogInformation("Database migrated successfully.");

            // Check if the database has been seeded
            if (context.Departments.Any() || context.Doctors.Any() || context.Patients.Any() || context.Medicines.Any())
            {
                logger.LogInformation("Database already seeded. Skipping seeding.");
                return; // DB has been seeded
            }

            try
            {
                // Seed Departments
                SeedDepartments(context, logger);

                // Seed Medicines
                SeedMedicines(context, logger);

                // Seed Doctors
                SeedDoctors(context, logger);

                // Seed Patients
                SeedPatients(context, logger);


                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding.");
                throw; // Rethrow the exception after logging
            }
        }

        private static void SeedDepartments(ApplicationDbContext context, ILogger logger)
        {
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "Cardiology" },
                new Department { Id = 2, Name = "Dermatology" },
                new Department { Id = 3, Name = "Gynecology" },
                new Department { Id = 4, Name = "Neurology" },
                new Department { Id = 5, Name = "Ophthalmology" },
                new Department { Id = 6, Name = "Pediatrics" },
                new Department { Id = 7, Name = "Psychiatry" },
                new Department { Id = 8, Name = "Radiology" },
                new Department { Id = 9, Name = "Surgery" },
                new Department { Id = 10, Name = "Emergency" }
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();
            logger.LogInformation("Seeded Departments.");
        }

        private static void SeedDoctors(ApplicationDbContext context, ILogger logger)
        {
            var doctors = new List<Doctor>
            {
                new Doctor
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    BirthDate = new DateTime(1975, 5, 20),
                    Gender = Gender.Male,
                    Email = "johndoe@hospital.com",
                    Phone = "555-0101",
                    Specialization = DoctorSpecialization.Cardiologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 1
                },
                new Doctor
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    BirthDate = new DateTime(1980, 8, 15),
                    Gender = Gender.Female,
                    Email = "janesmith@hospital.com",
                    Phone = "555-0102",
                    Specialization = DoctorSpecialization.Dermatologist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 2
                },
                new Doctor
                {
                    Id = 3,
                    FirstName = "Michael",
                    LastName = "Brown",
                    BirthDate = new DateTime(1978, 4, 12),
                    Gender = Gender.Male,
                    Email = "michaelbrown@hospital.com",
                    Phone = "555-0103",
                    Specialization = DoctorSpecialization.Gynecologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 3
                },
                new Doctor
                {
                    Id = 4,
                    FirstName = "Emily",
                    LastName = "Clark",
                    BirthDate = new DateTime(1985, 12, 5),
                    Gender = Gender.Female,
                    Email = "emilyclark@hospital.com",
                    Phone = "555-0104",
                    Specialization = DoctorSpecialization.Neurologist,
                    Status = DoctorStatus.OnLeave,
                    DepartmentId = 4
                },
                new Doctor
                {
                    Id = 5,
                    FirstName = "William",
                    LastName = "Davis",
                    BirthDate = new DateTime(1972, 7, 30),
                    Gender = Gender.Male,
                    Email = "williamdavis@hospital.com",
                    Phone = "555-0105",
                    Specialization = DoctorSpecialization.Ophthalmologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 5
                },
                new Doctor
                {
                    Id = 6,
                    FirstName = "Olivia",
                    LastName = "Martinez",
                    BirthDate = new DateTime(1990, 3, 25),
                    Gender = Gender.Female,
                    Email = "oliviamartinez@hospital.com",
                    Phone = "555-0106",
                    Specialization = DoctorSpecialization.Pediatrician,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 6
                },
                new Doctor
                {
                    Id = 7,
                    FirstName = "James",
                    LastName = "Garcia",
                    BirthDate = new DateTime(1979, 11, 10),
                    Gender = Gender.Male,
                    Email = "jamesgarcia@hospital.com",
                    Phone = "555-0107",
                    Specialization = DoctorSpecialization.Psychiatrist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 7
                },
                new Doctor
                {
                    Id = 8,
                    FirstName = "Sophia",
                    LastName = "Rodriguez",
                    BirthDate = new DateTime(1983, 6, 18),
                    Gender = Gender.Female,
                    Email = "sophiarodriguez@hospital.com",
                    Phone = "555-0108",
                    Specialization = DoctorSpecialization.Radiologist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 8
                },
                new Doctor
                {
                    Id = 9,
                    FirstName = "Benjamin",
                    LastName = "Lee",
                    BirthDate = new DateTime(1976, 2, 14),
                    Gender = Gender.Male,
                    Email = "benjaminlee@hospital.com",
                    Phone = "555-0109",
                    Specialization = DoctorSpecialization.Surgeon,
                    Status = DoctorStatus.Active,
                    DepartmentId = 9
                },
                new Doctor
                {
                    Id = 10,
                    FirstName = "Ava",
                    LastName = "Walker",
                    BirthDate = new DateTime(1988, 9, 5),
                    Gender = Gender.Female,
                    Email = "avawalker@hospital.com",
                    Phone = "555-0110",
                    Specialization = DoctorSpecialization.Cardiologist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 1
                },
                // Continue adding Doctors 11 to 20
                new Doctor
                {
                    Id = 11,
                    FirstName = "Daniel",
                    LastName = "Hall",
                    BirthDate = new DateTime(1974, 4, 22),
                    Gender = Gender.Male,
                    Email = "danielhall@hospital.com",
                    Phone = "555-0111",
                    Specialization = DoctorSpecialization.Dermatologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 2
                },
                new Doctor
                {
                    Id = 12,
                    FirstName = "Isabella",
                    LastName = "Young",
                    BirthDate = new DateTime(1982, 5, 17),
                    Gender = Gender.Female,
                    Email = "isabellayoung@hospital.com",
                    Phone = "555-0112",
                    Specialization = DoctorSpecialization.Gynecologist,
                    Status = DoctorStatus.OnLeave,
                    DepartmentId = 3
                },
                new Doctor
                {
                    Id = 13,
                    FirstName = "Matthew",
                    LastName = "Hernandez",
                    BirthDate = new DateTime(1979, 10, 8),
                    Gender = Gender.Male,
                    Email = "matthewhernandez@hospital.com",
                    Phone = "555-0113",
                    Specialization = DoctorSpecialization.Neurologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 4
                },
                new Doctor
                {
                    Id = 14,
                    FirstName = "Mia",
                    LastName = "King",
                    BirthDate = new DateTime(1986, 1, 12),
                    Gender = Gender.Female,
                    Email = "miaking@hospital.com",
                    Phone = "555-0114",
                    Specialization = DoctorSpecialization.Ophthalmologist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 5
                },
                new Doctor
                {
                    Id = 15,
                    FirstName = "Alexander",
                    LastName = "Wright",
                    BirthDate = new DateTime(1973, 8, 19),
                    Gender = Gender.Male,
                    Email = "alexanderwright@hospital.com",
                    Phone = "555-0115",
                    Specialization = DoctorSpecialization.Pediatrician,
                    Status = DoctorStatus.Active,
                    DepartmentId = 6
                },
                new Doctor
                {
                    Id = 16,
                    FirstName = "Charlotte",
                    LastName = "Lopez",
                    BirthDate = new DateTime(1992, 7, 3),
                    Gender = Gender.Female,
                    Email = "charlottelopez@hospital.com",
                    Phone = "555-0116",
                    Specialization = DoctorSpecialization.Psychiatrist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 7
                },
                new Doctor
                {
                    Id = 17,
                    FirstName = "Henry",
                    LastName = "Hill",
                    BirthDate = new DateTime(1981, 11, 27),
                    Gender = Gender.Male,
                    Email = "henryhill@hospital.com",
                    Phone = "555-0117",
                    Specialization = DoctorSpecialization.Radiologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 8
                },
                new Doctor
                {
                    Id = 18,
                    FirstName = "Amelia",
                    LastName = "Scott",
                    BirthDate = new DateTime(1977, 2, 5),
                    Gender = Gender.Female,
                    Email = "ameliascott@hospital.com",
                    Phone = "555-0118",
                    Specialization = DoctorSpecialization.Surgeon,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 9
                },
                new Doctor
                {
                    Id = 19,
                    FirstName = "Ethan",
                    LastName = "Green",
                    BirthDate = new DateTime(1984, 6, 29),
                    Gender = Gender.Male,
                    Email = "ethangreen@hospital.com",
                    Phone = "555-0119",
                    Specialization = DoctorSpecialization.Cardiologist,
                    Status = DoctorStatus.Active,
                    DepartmentId = 1
                },
                new Doctor
                {
                    Id = 20,
                    FirstName = "Olivia",
                    LastName = "Adams",
                    BirthDate = new DateTime(1978, 12, 15),
                    Gender = Gender.Female,
                    Email = "oliviaadams@hospital.com",
                    Phone = "555-0120",
                    Specialization = DoctorSpecialization.Dermatologist,
                    Status = DoctorStatus.OnDuty,
                    DepartmentId = 2
                }
            };

            context.Doctors.AddRange(doctors);
            context.SaveChanges();
            logger.LogInformation("Seeded Doctors.");
        }

        private static void SeedPatients(ApplicationDbContext context, ILogger logger)
        {
            var patients = new List<Patient>();
            for (int i = 1; i <= 40; i++)
            {
                patients.Add(new Patient
                {
                    Id = i,
                    FirstName = $"PatientFirstName{i}",
                    LastName = $"PatientLastName{i}",
                    DateOfBirth = new DateTime(1990 + (i % 10), (i % 12) + 1, (i % 28) + 1),
                    Gender = (i % 2 == 0) ? Gender.Male : Gender.Female,
                    PhoneNumber = $"555-020{i:D2}",
                    Email = $"patient{i}@hospital.com",
                    Address = $"{i} Hospital Street, City",
                    EmergencyContact = $"555-030{i:D2}",
                    DateTimeOfAdmission = DateTime.Now.AddDays(-i)
                });
            }

            context.Patients.AddRange(patients);
            context.SaveChanges();
            logger.LogInformation("Seeded Patients.");
        }

        private static void SeedMedicines(ApplicationDbContext context, ILogger logger)
        {
            var medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Paracetamol (Acetaminophen)", Description = "Pain reliever and fever reducer.", Price = 6.99M },
                new Medicine { Id = 2, Name = "Ibuprofen", Description = "Nonsteroidal anti-inflammatory drug (NSAID) used for pain relief.", Price = 8.99M },
                new Medicine { Id = 3, Name = "Amoxicillin", Description = "Antibiotic used to treat a variety of bacterial infections.", Price = 14.99M },
                new Medicine { Id = 4, Name = "Lisinopril", Description = "ACE inhibitor used to treat high blood pressure.", Price = 18.00M },
                new Medicine { Id = 5, Name = "Metformin", Description = "Oral diabetes medicine that helps control blood sugar levels.", Price = 12.50M },
                new Medicine { Id = 6, Name = "Atorvastatin", Description = "Statin medication used to prevent cardiovascular disease.", Price = 25.00M },
                new Medicine { Id = 7, Name = "Omeprazole", Description = "Proton pump inhibitor used to treat gastroesophageal reflux disease (GERD).", Price = 10.50M },
                new Medicine { Id = 8, Name = "Azithromycin", Description = "Antibiotic used for various bacterial infections.", Price = 16.75M },
                new Medicine { Id = 9, Name = "Ciprofloxacin", Description = "Antibiotic used to treat different types of bacterial infections.", Price = 11.99M },
                new Medicine { Id = 10, Name = "Gabapentin", Description = "Used to treat nerve pain and prevent seizures.", Price = 13.25M },
                new Medicine { Id = 11, Name = "Amlodipine", Description = "Calcium channel blocker used to treat high blood pressure and chest pain.", Price = 17.00M },
                new Medicine { Id = 12, Name = "Hydrochlorothiazide", Description = "Diuretic used to treat high blood pressure and fluid retention.", Price = 7.50M },
                new Medicine { Id = 13, Name = "Sertraline", Description = "Selective serotonin reuptake inhibitor (SSRI) used to treat depression and anxiety.", Price = 19.00M },
                new Medicine { Id = 14, Name = "Tramadol", Description = "Opioid pain medication used to treat moderate to severe pain.", Price = 22.00M },
                new Medicine { Id = 15, Name = "Alprazolam", Description = "Benzodiazepine used to manage anxiety disorders.", Price = 20.50M },
                new Medicine { Id = 16, Name = "Prednisone", Description = "Corticosteroid used to reduce inflammation.", Price = 19.00M },
                new Medicine { Id = 17, Name = "Furosemide", Description = "Loop diuretic used to treat fluid build-up due to heart failure, liver scarring, or kidney disease.", Price = 8.25M },
                new Medicine { Id = 18, Name = "Levothyroxine", Description = "Synthetic thyroid hormone used to treat hypothyroidism.", Price = 6.00M },
                new Medicine { Id = 19, Name = "Cyclobenzaprine", Description = "Muscle relaxant used to relieve muscle spasms.", Price = 14.00M },
                new Medicine { Id = 20, Name = "Clopidogrel", Description = "Antiplatelet agent used to prevent blood clots.", Price = 24.00M }
                // Add more medicines if needed
            };

            context.Medicines.AddRange(medicines);
            context.SaveChanges();
            logger.LogInformation("Seeded Medicines.");
        }
    }
}
