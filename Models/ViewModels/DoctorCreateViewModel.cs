using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.ViewModels
{
    public class DoctorCreateViewModel
    {
        // Readonly field for Doctor's Name
        public string DoctorName { get; set; }

        // Dropdown for Patients
        [Display(Name = "Patient")]
        [Required]
        public int SelectedPatientId { get; set; }
        public IEnumerable<SelectListItem> Patients { get; set; }

        // Appointment Date Selection
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime AppointmentDate { get; set; }

        // Appointment Status Dropdown
        [Display(Name = "Appointment Status")]
        [Required]
        public AppointmentStatus SelectedAppointmentStatus { get; set; }
        public IEnumerable<SelectListItem> AppointmentStatuses { get; set; }

        // Notifications
        public int DoctorNotification { get; set; } = 0;
        public int PatientNotification { get; set; } = 0;
    }
}
