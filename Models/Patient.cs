using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; } // Enum type

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }

        public string EmergencyContact { get; set; }

        [Display(Name = "Date and Time of Admission")]
        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfAdmission { get; set; }

        // Foreign Key to Primary Doctor
        public int? PrimaryDoctorId { get; set; }
        public Doctor PrimaryDoctor { get; set; }

        // Navigation Property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Foreign Key to IdentityUser
        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }
    }
}
