using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.ViewModels
{
    public class PatientCreateViewModel
    {
        // Readonly field for Patient's Name
        public string PatientName { get; set; }

        // Dropdown for Doctors
        [Display(Name = "Primary Doctor")]
        [Required]
        public int SelectedDoctorId { get; set; }
        public IEnumerable<SelectListItem> Doctors { get; set; }

        // Appointment Date Selection
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime AppointmentDate { get; set; }

        // Hidden/Background Fields
        public float BillAmount { get; set; }
        public BillStatus BillStatus { get; set; }
        public FeedbackStatus FeedbackStatus { get; set; }
        public string Feedback { get; set; }

        // Notifications
        public int DoctorNotification { get; set; } = 0;
        public int PatientNotification { get; set; } = 0;
    }
}
