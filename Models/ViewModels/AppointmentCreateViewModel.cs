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
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Appointment Status")]
        public AppointmentStatus AppointmentStatus { get; set; }

        [Required]
        [Display(Name = "Bill Status")]
        public BillStatus BillStatus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Bill amount must be non-negative.")]
        [Display(Name = "Bill Amount")]
        public float BillAmount { get; set; }

        // Admins can select patient
        [Display(Name = "Patient")]
        public int? SelectedPatientId { get; set; }

        // Admins and Doctors can select multiple doctors
        [Display(Name = "Doctors")]
        public List<int> SelectedDoctorIds { get; set; } = new List<int>();

        // Dropdown lists
        public List<SelectListItem> DoctorsList { get; set; }
        public List<SelectListItem> PatientsList { get; set; }

        // For Patients, auto-assign
        public int? PatientId { get; set; }

        // Select lists for AppointmentStatus and BillStatus
        public IEnumerable<SelectListItem> AppointmentStatusList => Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>().Select(status => new SelectListItem
        {
            Value = status.ToString(),
            Text = status.ToString()
        });

        public IEnumerable<SelectListItem> BillStatusList => Enum.GetValues(typeof(BillStatus)).Cast<BillStatus>().Select(status => new SelectListItem
        {
            Value = status.ToString(),
            Text = status.ToString()
        });
    }
}
