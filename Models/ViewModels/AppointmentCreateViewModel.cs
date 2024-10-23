// Models/ViewModels/AppointmentCreateViewModel.cs
using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hospital_Managment_System.Models.ViewModels
{
    public class AppointmentCreateViewModel
    {
        // Common Properties
        [Required]
        public DateTime AppointmentDate { get; set; }

        // For Doctors
        public int? PatientId { get; set; }

        // For Patients
        public string PatientFullName { get; set; }

        // Doctor Selection
        [Required]
        [Display(Name = "Doctor")]
        public int SelectedDoctorId { get; set; }
        public List<SelectListItem> DoctorList { get; set; }

        // Enum Selections
        public AppointmentStatus AppointmentStatus { get; set; }
        public List<SelectListItem> AppointmentStatusList { get; set; }

        public BillStatus BillStatus { get; set; }
        public List<SelectListItem> BillStatusList { get; set; }

        // Notifications and Feedback
        public int PatientNotification { get; set; }
        public int DoctorNotification { get; set; }
        public FeedbackStatus FeedbackStatus { get; set; }
    }
}
