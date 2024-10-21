using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }


        public Gender? Gender { get; set; } // Changed to enum type


        [DataType(DataType.PhoneNumber)]
        public required string PhoneNumber { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string EmergencyContact { get; set; }


        [Display(Name = "Date and Time of Admission")]
        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfAdmission { get; set; }


        // Foreign Key to Primary Doctor
        [ForeignKey("PrimaryDoctor")]
        public int? PrimaryDoctorId { get; set; }
        public Doctor? PrimaryDoctor { get; set; }


        // Navigation Property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Foreign Key to IdentityUser
        
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

    }
}
