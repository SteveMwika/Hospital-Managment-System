using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        public DoctorSpecialization Specialization { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public DoctorStatus Status { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // Navigation Properties
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Foreign Key to IdentityUser
        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }
    }
}
