// Data/SeedData.cs
using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital_Managment_System.Data
{
    public static class SeedData
    {
        private static readonly List<string> maleFirstNames = new List<string>
        {
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Charles", "Thomas",
            "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Paul", "Steven", "Andrew", "Kenneth",
            "George", "Joshua", "Kevin", "Brian", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan",
            "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon",
            "Benjamin", "Samuel", "Gregory", "Frank", "Alexander", "Patrick", "Raymond", "Jack", "Dennis", "Jerry"
        };

        private static readonly List<string> femaleFirstNames = new List<string>
        {
            "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen",
            "Nancy", "Lisa", "Margaret", "Betty", "Sandra", "Ashley", "Dorothy", "Kimberly", "Emily", "Donna",
            "Michelle", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Sharon", "Laura", "Cynthia",
            "Kathleen", "Amy", "Shirley", "Angela", "Helen", "Anna", "Brenda", "Pamela", "Nicole", "Samantha",
            "Katherine", "Emma", "Ruth", "Christine", "Catherine", "Debra", "Rachel", "Carolyn", "Janet", "Maria"
        };
        private static readonly List<string> lastNames = new List<string>
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
            "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
            "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
            "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
        };

        private static readonly List<string> streetNames = new List<string>
        {
            "Maple Street", "Oak Avenue", "Cedar Lane", "Pine Road", "Elm Drive", "Sunset Boulevard", "Ridgeview Terrace", "Broadway",
            "Main Street", "Highland Avenue", "Lincoln Street", "Washington Avenue", "Madison Drive", "Jefferson Road", "Adams Boulevard",
            "Franklin Street", "Jackson Avenue", "Wilson Lane", "Park Street", "Hillcrest Avenue", "Forest Drive", "Lakeview Road",
            "River Street", "Meadow Lane", "Cherry Street", "Dogwood Road", "Spruce Street", "Magnolia Avenue", "Sycamore Lane",
            "Birch Drive", "Laurel Street", "Juniper Road", "Ash Avenue", "Willow Lane", "Hickory Drive", "Poplar Road", "Chestnut Street",
            "Beech Avenue", "Alder Road", "Mulberry Street", "Holly Lane", "Ivy Drive", "Rose Street", "Tulip Road", "Violet Lane",
            "Lily Street", "Daisy Avenue", "Orchid Drive", "Gladiolus Lane", "Fountain Avenue", "Pacific Boulevard", "Ocean Drive",
            "Palm Street", "Crescent Lane", "Valley Drive", "Eagle Road", "Fox Street", "Bear Avenue", "Hawk Lane", "Wolf Road",
            "Deer Street", "Fawn Lane", "Raven Avenue", "Dove Road", "Swan Drive", "Otter Lane", "Beaver Road", "Cougar Street",
            "Panther Avenue", "Tiger Lane", "Lion Road", "Giraffe Street", "Elephant Avenue", "Zebra Lane", "Buffalo Road",
            "Falcon Street", "Owl Avenue", "Elk Lane", "Moose Road", "Shady Lane", "Sunrise Street", "Mountain View Road",
            "Hilltop Avenue", "Waterfall Drive", "Canyon Road", "Cliffside Street", "Desert Drive", "Forest Lane", "Bayview Avenue",
            "Lakeshore Road", "Harbor Street", "Beacon Avenue", "Shores Drive", "Tidewater Lane", "Blossom Street", "Vista Avenue",
            "Echo Drive", "Summit Street", "Riverside Avenue", "Bayside Road", "Coastal Drive", "Evergreen Street"
        };

        private static readonly List<string> usStates = new List<string>
        {
            "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia",
            "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland",
            "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
            "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania",
            "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington",
            "West Virginia", "Wisconsin", "Wyoming"
        };

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Obtain RoleManager and UserManager instances
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Obtain ApplicationDbContext instance
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed roles
            await EnsureRolesAsync(roleManager);

            // Seed departments and medicines
            await EnsureDepartmentsAsync(context);
            await EnsureMedicinesAsync(context);

            // Seed users and associate them with doctors and patients
            await EnsureAdminUserAsync(userManager);
            await EnsureDoctorUsersAsync(userManager, context);
            await EnsurePatientUsersAsync(userManager, context);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "Doctor", "Patient" };

            foreach (var roleName in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task EnsureAdminUserAsync(UserManager<IdentityUser> userManager)
        {
            var adminEmail = "admin@hospital.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!"); // Use a secure password

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    // Handle errors
                }
            }
        }

        private static async Task EnsureDoctorUsersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            // Map specializations to departments
            var specializationDepartmentMap = new Dictionary<DoctorSpecialization, string>
            {
                // Department of Internal Medicine
                { DoctorSpecialization.Cardiologist, "Internal Medicine" },
                { DoctorSpecialization.Endocrinologist, "Internal Medicine" },
                { DoctorSpecialization.Gastroenterologist, "Internal Medicine" },
                { DoctorSpecialization.Hematologist, "Internal Medicine" },
                { DoctorSpecialization.Immunologist, "Internal Medicine" },
                { DoctorSpecialization.InfectiousDisease, "Internal Medicine" },
                { DoctorSpecialization.Nephrologist, "Internal Medicine" },
                { DoctorSpecialization.Pulmonologist, "Internal Medicine" },
                { DoctorSpecialization.Rheumatologist, "Internal Medicine" },
                { DoctorSpecialization.Allergist, "Internal Medicine" },
                { DoctorSpecialization.Hepatologist, "Internal Medicine" },
                { DoctorSpecialization.Geriatrician, "Internal Medicine" },
                { DoctorSpecialization.PalliativeCare, "Internal Medicine" },

                // Department of Surgery
                { DoctorSpecialization.Surgeon, "Surgery" },
                { DoctorSpecialization.OrthopedicSurgeon, "Surgery" },
                { DoctorSpecialization.PlasticSurgeon, "Surgery" },
                { DoctorSpecialization.VascularSurgeon, "Surgery" },
                { DoctorSpecialization.ThoracicSurgeon, "Surgery" },
                { DoctorSpecialization.BariatricSurgeon, "Surgery" },
                { DoctorSpecialization.Urologist, "Surgery" },
                { DoctorSpecialization.Otolaryngologist, "Surgery" },

                // Department of Pediatrics
                { DoctorSpecialization.Pediatrician, "Pediatrics" },
                { DoctorSpecialization.Neonatologist, "Pediatrics" },
                { DoctorSpecialization.PediatricSurgeon, "Pediatrics" }, // Assigned to Pediatrics

                // Department of Obstetrics and Gynecology
                { DoctorSpecialization.Gynecologist, "Obstetrics and Gynecology" },
                { DoctorSpecialization.Obstetrician, "Obstetrics and Gynecology" },

                // Department of Neurology and Psychiatry
                { DoctorSpecialization.Neurologist, "Neurology and Psychiatry" },
                { DoctorSpecialization.Psychiatrist, "Neurology and Psychiatry" },
                { DoctorSpecialization.SleepMedicine, "Neurology and Psychiatry" }, // Assigned here

                // Department of Radiology and Nuclear Medicine
                { DoctorSpecialization.Radiologist, "Radiology and Nuclear Medicine" },
                { DoctorSpecialization.NuclearMedicine, "Radiology and Nuclear Medicine" },

                // Department of Anesthesiology and Pain Management
                { DoctorSpecialization.Anesthesiologist, "Anesthesiology and Pain Management" },

                // Department of Emergency and Critical Care Medicine
                { DoctorSpecialization.EmergencyMedicine, "Emergency and Critical Care" },
                { DoctorSpecialization.CriticalCare, "Emergency and Critical Care" }, // Assigned here

                // Department of Pathology and Laboratory Medicine
                { DoctorSpecialization.Pathologist, "Pathology and Laboratory Medicine" },
                { DoctorSpecialization.MedicalGenetics, "Pathology and Laboratory Medicine" }, // Assigned here

                // Department of Specialized Medicine
                { DoctorSpecialization.Dermatologist, "Specialized Medicine" },
                { DoctorSpecialization.Ophthalmologist, "Specialized Medicine" },
                { DoctorSpecialization.Dentist, "Specialized Medicine" },
                { DoctorSpecialization.SportsMedicine, "Specialized Medicine" },
                { DoctorSpecialization.OccupationalMedicine, "Specialized Medicine" }, // Assigned here
            };

            var departments = context.Departments.ToList();
            int totalDoctors = 40;
            int specializationCount = Enum.GetValues(typeof(DoctorSpecialization)).Length;
            Random random = new Random();

            for (int i = 1; i <= totalDoctors; i++)
            {
                var email = $"doctor{i}@hospital.com";
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, $"DoctorPassword{i}!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");

                        // Assign specialization and department
                        var specialization = (DoctorSpecialization)(((i - 1) % specializationCount) + 1);
                        var departmentName = specializationDepartmentMap.ContainsKey(specialization)
                            ? specializationDepartmentMap[specialization]
                            : "General Medicine";

                        var department = departments.FirstOrDefault(d => d.Name == departmentName) ?? departments.First();

                        // Randomly assign gender
                        var gender = (Gender)(((i - 1) % Enum.GetValues(typeof(Gender)).Length) + 1);

                        // Select first name based on gender
                        string firstName = gender == Gender.Male
                            ? maleFirstNames[random.Next(maleFirstNames.Count)]
                            : femaleFirstNames[random.Next(femaleFirstNames.Count)];

                        // Select a random last name
                        string lastName = lastNames[random.Next(lastNames.Count)];

                        // Create Doctor entity
                        var doctor = new Doctor
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            BirthDate = new DateTime(1970 + (i % 30), (i % 12) + 1, (i % 28) + 1),
                            Gender = gender,
                            Email = email,
                            Phone = $"555-01{i:D2}",
                            Specialization = specialization,
                            Status = (DoctorStatus)(((i - 1) % Enum.GetValues(typeof(DoctorStatus)).Length) + 1),
                            DepartmentId = department.Id,
                            UserId = user.Id
                        };
                        context.Doctors.Add(doctor);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task EnsurePatientUsersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            var doctors = context.Doctors.ToList();
            int totalPatients = 80;
            Random random = new Random();

            // Create a dictionary to keep track of how many patients each doctor has
            var doctorPatientCount = doctors.ToDictionary(d => d.Id, d => 0);

            for (int i = 1; i <= totalPatients; i++)
            {
                var email = $"patient{i}@hospital.com";
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, $"PatientPassword{i}!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Patient");

                        // Randomly assign a doctor while ensuring max 20 patients per doctor
                        Doctor primaryDoctor;
                        do
                        {
                            primaryDoctor = doctors[random.Next(doctors.Count)];
                        } while (doctorPatientCount[primaryDoctor.Id] >= 20);

                        // Increment the patient's count for the assigned doctor
                        doctorPatientCount[primaryDoctor.Id]++;

                        // Randomly assign gender
                        var gender = (Gender)(((i - 1) % Enum.GetValues(typeof(Gender)).Length) + 1);

                        // Select first name based on gender
                        string firstName = gender == Gender.Male
                            ? maleFirstNames[random.Next(maleFirstNames.Count)]
                            : femaleFirstNames[random.Next(femaleFirstNames.Count)];

                        // Select a random last name
                        string lastName = lastNames[random.Next(lastNames.Count)];

                        // Unique address generation
                        var address = $"{i} {streetNames[i % streetNames.Count]}, {usStates[i % usStates.Count]} City, {i:D5}";

                        // Create Patient entity
                        var patient = new Patient
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            DateOfBirth = new DateTime(1990 - i % 30, (i % 12) + 1, (i % 28) + 1),
                            Gender = gender,
                            PhoneNumber = $"555-020{i:D2}",
                            Email = email,
                            Address = address,
                            EmergencyContact = $"555-030{i:D2}",
                            DateTimeOfAdmission = DateTime.UtcNow.AddDays(-i),
                            UserId = user.Id,
                            PrimaryDoctorId = primaryDoctor.Id
                        };
                        context.Patients.Add(patient);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task EnsureDepartmentsAsync(ApplicationDbContext context)
        {
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Internal Medicine" },
                    new Department { Name = "Surgery" },
                    new Department { Name = "Pediatrics" },
                    new Department { Name = "Obstetrics and Gynecology" },
                    new Department { Name = "Neurology and Psychiatry" },
                    new Department { Name = "Radiology and Nuclear Medicine" },
                    new Department { Name = "Anesthesiology and Pain Management" },
                    new Department { Name = "Emergency and Critical Care" },
                    new Department { Name = "Pathology and Laboratory Medicine" },
                    new Department { Name = "Specialized Medicine" },
                };

                context.Departments.AddRange(departments);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureMedicinesAsync(ApplicationDbContext context)
        {
            if (!context.Medicines.Any())
            {
                var medicines = new List<Medicine>
                {
                    new Medicine { Name = "Paracetamol (Acetaminophen)", Description = "Pain reliever and fever reducer.", Price = 6.99m, Quantity = 100 },
                    new Medicine { Name = "Ibuprofen", Description = "NSAID used for pain relief.", Price = 8.99m, Quantity = 100 },
                    new Medicine { Name = "Amoxicillin", Description = "Antibiotic used to treat bacterial infections.", Price = 14.99m, Quantity = 100 },
                    new Medicine { Name = "Lisinopril", Description = "Used to treat high blood pressure.", Price = 18.00m, Quantity = 100 },
                    new Medicine { Name = "Metformin", Description = "Helps control blood sugar levels.", Price = 12.50m, Quantity = 100 },
                    new Medicine { Name = "Atorvastatin", Description = "Used to prevent cardiovascular disease.", Price = 25.00m, Quantity = 100 },
                    new Medicine { Name = "Omeprazole", Description = "Used to treat GERD.", Price = 10.50m, Quantity = 100 },
                    new Medicine { Name = "Azithromycin", Description = "Antibiotic for bacterial infections.", Price = 16.75m, Quantity = 100 },
                    new Medicine { Name = "Ciprofloxacin", Description = "Antibiotic for bacterial infections.", Price = 11.99m, Quantity = 100 },
                    new Medicine { Name = "Gabapentin", Description = "Treats nerve pain and seizures.", Price = 13.25m, Quantity = 100 },
                    new Medicine { Name = "Amlodipine", Description = "Treats high blood pressure.", Price = 17.00m, Quantity = 100 },
                    new Medicine { Name = "Hydrochlorothiazide", Description = "Treats high blood pressure.", Price = 7.50m, Quantity = 100 },
                    new Medicine { Name = "Sertraline", Description = "Treats depression and anxiety.", Price = 19.00m, Quantity = 100 },
                    new Medicine { Name = "Tramadol", Description = "Treats moderate to severe pain.", Price = 22.00m, Quantity = 100 },
                    new Medicine { Name = "Alprazolam", Description = "Manages anxiety disorders.", Price = 20.50m, Quantity = 100 },
                    new Medicine { Name = "Prednisone", Description = "Reduces inflammation.", Price = 19.00m, Quantity = 100 },
                    new Medicine { Name = "Furosemide", Description = "Treats fluid build-up.", Price = 8.25m, Quantity = 100 },
                    new Medicine { Name = "Levothyroxine", Description = "Treats hypothyroidism.", Price = 6.00m, Quantity = 100 },
                    new Medicine { Name = "Cyclobenzaprine", Description = "Relieves muscle spasms.", Price = 14.00m, Quantity = 100 },
                    new Medicine { Name = "Clopidogrel", Description = "Prevents blood clots.", Price = 24.00m, Quantity = 100 }
                };

                context.Medicines.AddRange(medicines);
                await context.SaveChangesAsync();

                // Create initial inventory logs for medicines
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hospital.com");

                foreach (var medicine in medicines)
                {
                    var inventoryLog = new MedicineInventoryLog
                    {
                        MedicineId = medicine.Id,
                        ChangeType = MedicineChangeType.Added,
                        QuantityChanged = medicine.Quantity,
                        NewQuantity = medicine.Quantity,
                        DateTime = DateTime.UtcNow,
                        AdminUserId = adminUser.Id,
                        Description = "Initial stock added."
                    };

                    context.MedicineInventoryLogs.Add(inventoryLog);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
