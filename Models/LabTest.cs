using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class LabTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string TestName { get; set; }

        [StringLength(500)]
        public string Result { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TestDate { get; set; }

        [Required]
        public bool IsCompleted { get; set; }

        // Navigation Property
        public Appointment Appointment { get; set; }
    }
}
