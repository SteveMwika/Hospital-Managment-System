namespace Hospital_Managment_System.Enums
{
    public enum UserRoles
    {
        Patient = 1,
        Doctor = 2,
        Admin = 3
    }

    public enum AppointmentStatus
    {
        Approved = 1,
        Pending = 2,
        Completed = 3,
        DrCancelled = 4,
        PatientCancelled = 5,
        Scheduled = 6
    }

    public enum BillStatus
    {
        Paid = 1,
        Unpaid = 2
    }

    public enum MedicineChangeType
    {
        Added = 1,
        Removed = 2,
        Prescribed = 3,
        QuantityAdjusted = 4,
        Deleted = 5
    }

    public enum DoctorSpecialization
    {
        Cardiologist = 1,            // Heart specialist
        Dermatologist = 2,           // Skin specialist
        Gynecologist = 3,            // Female reproductive health
        Neurologist = 4,             // Nervous system specialist
        Ophthalmologist = 5,         // Eye specialist
        Pediatrician = 6,            // Children's health
        Psychiatrist = 7,            // Mental health specialist
        Radiologist = 8,             // Imaging specialist
        Surgeon = 9,                 // General surgeon

        // Additional Specializations
        Anesthesiologist = 10,       // Pain relief and anesthesia management
        Endocrinologist = 11,        // Hormone and metabolism specialist
        Gastroenterologist = 12,     // Digestive system specialist
        Hematologist = 13,           // Blood disorder specialist
        Immunologist = 14,           // Immune system specialist
        InfectiousDisease = 15,      // Infection specialist
        Nephrologist = 16,           // Kidney specialist
        Oncologist = 17,             // Cancer specialist
        OrthopedicSurgeon = 18,      // Bone and joint specialist
        Pathologist = 19,            // Disease diagnosis via lab tests
        PlasticSurgeon = 20,         // Reconstructive surgery specialist
        Pulmonologist = 21,          // Lung and respiratory specialist
        Rheumatologist = 22,         // Autoimmune and joint disease specialist
        Urologist = 23,              // Urinary tract and male reproductive specialist
        Otolaryngologist = 24,       // Ear, nose, and throat specialist
        Neonatologist = 25,          // Newborn baby specialist
        VascularSurgeon = 26,        // Specialist in blood vessel surgeries
        Geriatrician = 27,           // Specialist in elderly care
        Allergist = 28,              // Allergy specialist
        Dentist = 29,                // Oral health specialist
        Obstetrician = 30,           // Pregnancy and childbirth specialist
        Hepatologist = 31,           // Liver specialist
        ThoracicSurgeon = 32,        // Chest and lung surgery specialist
        EmergencyMedicine = 33,      // Emergency and critical care specialist
        FamilyMedicine = 34,         // General practice across all ages
        SportsMedicine = 35,         // Injury prevention and treatment for athletes
        BariatricSurgeon = 36,       // Specialist in obesity surgery
        PalliativeCare = 37,         // End-of-life care specialist
        PediatricSurgeon = 38,       // Surgical specialist for children
        CriticalCare = 39,           // Intensive care unit (ICU) specialist
        OccupationalMedicine = 40,   // Workplace health and injury prevention
        MedicalGenetics = 41,        // Genetic disorder specialist
        SleepMedicine = 42,          // Sleep disorder specialist
        NuclearMedicine = 43,        // Radioactive treatments and diagnostics
    }

    public enum DoctorStatus
    {
        Active = 1,
        OnLeave = 2,
        OnDuty = 3
    }

    public enum NotificationStatus
    {
        Read = 1,
        Unread = 2
    }

    public enum FeedbackStatus
    {
        Given = 1,
        Pending = 2
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum LabTestName
    {
        CompleteBloodCount = 1,         // CBC
        BasicMetabolicPanel = 2,        // BMP
        ComprehensiveMetabolicPanel = 3, // CMP
        LipidPanel = 4,                 // Cholesterol Test
        LiverFunctionTest = 5,          // LFT
        ThyroidFunctionTest = 6,        // TFT
        HemoglobinA1c = 7,              // Diabetes Test
        Urinalysis = 8,                 // UA
        BloodGlucose = 9,               // Glucose Test
        ProthrombinTime = 10,           // PT/INR
        DDimers = 11,                   // D-Dimer Test
        CReactiveProtein = 12,          // CRP
        RheumatoidFactor = 13,          // RF Test
        ErythrocyteSedimentationRate = 14, // ESR Test
        IronStudies = 15,               // Ferritin, Iron Levels
        VitaminDTest = 16,              // 25-hydroxy Vitamin D
        VitaminB12Test = 17,            // B12 Levels
        ElectrolytePanel = 18,          // Sodium, Potassium, etc.
        ArterialBloodGas = 19,          // ABG Test
        BloodUreaNitrogen = 20          // BUN Test
    }

    public enum PaymentMethodType
    {
        Cash = 1,
        CreditCard = 2,
        DebitCard = 3,
        Insurance = 4,
        BankTransfer = 5,
        MobilePayment = 6,  // Such as PayPal, Apple Pay, Google Pay
        Cheque = 7
    }


}
