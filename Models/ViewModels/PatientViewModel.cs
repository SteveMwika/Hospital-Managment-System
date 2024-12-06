using Hospital_Managment_System.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models.ViewModels
{
    public class PatientViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First Name cannot be longer than 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot be longer than 100 characters.")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [StringLength(15, ErrorMessage = "Phone Number cannot be longer than 15 characters.")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters.")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Emergency Contact")]
        [StringLength(15, ErrorMessage = "Emergency Contact cannot be longer than 15 characters.")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Date and Time of Admission")]
        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfAdmission { get; set; }

        [Display(Name = "Primary Doctor")]
        public int? PrimaryDoctorId { get; set; }

        // Password is required only during creation
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public int DepartmentId { get; set; }
    }
}
