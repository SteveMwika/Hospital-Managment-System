using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AppointmentID")]
        public int AppointmentID { get; set; }
        public Appointment Appointment { get; set; }

        [ForeignKey("MedicineID")]
        public int MedicineID { get; set; }
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
