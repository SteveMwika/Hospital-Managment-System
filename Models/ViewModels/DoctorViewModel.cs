using Hospital_Managment_System.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models.ViewModels
{
    public class DoctorViewModel
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

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        [StringLength(15, ErrorMessage = "Phone cannot be longer than 15 characters.")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Specialization")]
        public DoctorSpecialization Specialization { get; set; }

        [Required]
        [Display(Name = "Status")]
        public DoctorStatus Status { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        // Password fields are only required when creating a new doctor
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Flag to indicate if we are editing or creating a new user
        public bool IsEditMode { get; set; } = false;
    }
}
