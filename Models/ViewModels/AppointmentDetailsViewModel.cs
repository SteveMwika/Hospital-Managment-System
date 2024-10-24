// Models/ViewModels/AppointmentDetailsViewModel.cs
using Hospital_Managment_System.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models.ViewModels
{
    public class AppointmentDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Appointment Status")]
        public AppointmentStatus AppointmentStatus { get; set; }

        [Display(Name = "Bill Status")]
        public BillStatus BillStatus { get; set; }

        [Display(Name = "Bill Amount")]
        public float BillAmount { get; set; }

        [Display(Name = "Patient")]
        public string PatientName { get; set; }

        [Display(Name = "Doctors")]
        public List<string> DoctorNames { get; set; }

        [Display(Name = "Doctor Notification")]
        public bool DoctorNotification { get; set; }

        [Display(Name = "Patient Notification")]
        public bool PatientNotification { get; set; }

        [Display(Name = "Feedback Status")]
        public FeedbackStatus FeedbackStatus { get; set; }

        // Related Entities
        public List<Prescription> Prescriptions { get; set; }
        public List<LabTest> LabTests { get; set; }
    }
}