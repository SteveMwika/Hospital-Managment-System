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

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Sex is required.")]
        public Gender? Gender { get; set; } // Enum type

        [Required(ErrorMessage = "Phone Number is required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone Number must be in the format 555-555-5555.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Emergency Contact is required.")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Emergency Contact must be in the format 555-555-5555.")]
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
