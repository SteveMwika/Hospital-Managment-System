using Hospital_Managment_System.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class LabTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }  // Foreign key to the Appointment table

        [Required]
        public LabTestName TestName { get; set; }  // Enum representing the test name

        public string TestResult { get; set; }  // The generated result for the test

        [Required]
        [DataType(DataType.Date)]
        public DateTime TestDate { get; set; }  // Date when the test was conducted

        [Required]
        public bool IsCompleted { get; set; }  // Marks if the test is complete or not

        public string Comments { get; set; }  // Any additional comments about the test

        // Navigation Property
        public Appointment Appointment { get; set; }  // The associated appointment
    }
}
