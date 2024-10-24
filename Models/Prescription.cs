using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [Required]
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }

        [Required]
        [StringLength(200)]
        public string Instructions { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}
