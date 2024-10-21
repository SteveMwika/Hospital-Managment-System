using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class LabTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AppointmentID")]
        public int AppointmentID { get; set; }
        public Appointment Appointment { get; set; }

        [Required]
        [StringLength(200)]
        public string TestName { get; set; }

        public string Result { get; set; }

        [Required]
        public DateTime TestDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}
