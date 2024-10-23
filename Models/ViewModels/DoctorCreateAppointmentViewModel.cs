using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.ViewModels
{
    public class DoctorCreateAppointmentViewModel
    {
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        public SelectList PatientsList { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Appointment Status")]
        public AppointmentStatus AppointmentStatus { get; set; }
    }
}
