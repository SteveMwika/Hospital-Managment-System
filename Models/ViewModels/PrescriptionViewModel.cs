using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models.ViewModels
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Appointment")]
        public int AppointmentID { get; set; }

        [Required]
        [Display(Name = "Medicine")]
        public int MedicineID { get; set; }

        [Required]
        [Display(Name = "Instructions")]
        [StringLength(500, ErrorMessage = "Instructions cannot be longer than 500 characters.")]
        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }
    }
}
