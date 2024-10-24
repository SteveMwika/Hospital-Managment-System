using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.ViewModels
{
    public class PatientCreateAppointmentViewModel
    {
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        public SelectList DoctorsList { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }
    }
}
