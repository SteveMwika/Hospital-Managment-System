using Hospital_Managment_System.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AppointmentId")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public BillStatus Status { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }



    }
}
