using Hospital_Managment_System.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("PatientId")]
        public int PatientId { get; set; }

        [Required]
        public required Patient Patient { get; set; } // Required navigation property

        [Required]
        public required DateTime AppointmentDate { get; set; }

        [Required]
        public required AppointmentStatus AppointmentStatus { get; set; }

        public float BillAmount { get; set; }

        [Required]
        [StringLength(10)]
        public required string BillStatus { get; set; }

        public int DoctorNotification { get; set; }
        public int PatientNotification { get; set; }

        public FeedbackStatus FeedbackStatus { get; set; }



        // Navigation Properties
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public Billing Billing { get; set; }

        public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();

        // Many-to-Many relationship with Doctors
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
