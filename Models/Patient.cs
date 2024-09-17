using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Patient
    {
        public int id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name ="Last Name")]
        public string LastName { get; set; }


        [Display(Name ="Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }
        public string  Gender { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        [Display(Name = "Date and Time of Admission")]
        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfAdmission { get; set; }


        //foreign key to Medical record
        public MedicalRecord MedicalRecord { get; set; }

    }
}
