using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public DoctorSpecialization Specialization { get; set; }

        [Required]
        public DoctorStatus Status { get; set; }

        [Required]
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
