using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Doctor
    {
        public int id { get; set; }
        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

    }
}
