using Hospital_Managment_System.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public Patient Patient { get; set; } // Required navigation property

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public AppointmentStatus AppointmentStatus { get; set; }

        public float BillAmount { get; set; }

        /*public BillStatus BillStatus { get; set; }*/
        // Removed BillStatus string property

        public int DoctorNotification { get; set; }
        public int PatientNotification { get; set; }

        public FeedbackStatus FeedbackStatus { get; set; }

        [StringLength(200)]
        public string Feedback { get; set; }

        public string LabReport { get; set; }

        // Navigation Properties
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public Billing Billing { get; set; }

        public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();

        // Many-to-Many relationship with Doctors
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
