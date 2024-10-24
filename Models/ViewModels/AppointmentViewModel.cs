using Hospital_Managment_System.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Appointment Status")]
        public AppointmentStatus AppointmentStatus { get; set; }

        [Display(Name = "Bill Amount")]
        public float BillAmount { get; set; }

        [Required]
        [Display(Name = "Bill Status")]
        public BillStatus BillStatus { get; set; }

        [Display(Name = "Feedback Status")]
        public FeedbackStatus FeedbackStatus { get; set; }

        [Display(Name = "Doctor Notifications")]
        public int DoctorNotification { get; set; }

        [Display(Name = "Patient Notifications")]
        public int PatientNotification { get; set; }

        [Display(Name = "Doctors")]
        public List<int> SelectedDoctorIds { get; set; } = new List<int>();

        // Additional properties can be added as needed
    }
}
