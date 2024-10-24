using Hospital_Managment_System.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Managment_System.Models
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to the Appointment table
        [Required]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        // Billing amount with appropriate decimal precision
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Billing status (e.g., Paid, Unpaid)
        [Required]
        public BillStatus Status { get; set; }

        // Date when the billing was created
        [Required]
        [DataType(DataType.Date)]
        public DateTime BillingDate { get; set; }

        // Payment method (e.g., CreditCard, Cash, Insurance)
        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        // Due date for payment (applicable to pending or unpaid bills)
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // Payment date for tracking when the bill was paid
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }
    }
}
