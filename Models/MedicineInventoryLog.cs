using Hospital_Managment_System.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class MedicineInventoryLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }

        [Required]
        public MedicineChangeType ChangeType { get; set; }

        [Required]
        public int QuantityChanged { get; set; }

        [Required]
        public int NewQuantity { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        // Nullable foreign keys
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }

        // For admin actions
        public string AdminUserId { get; set; }
        public IdentityUser? AdminUser { get; set; }

        public string Description { get; set; }
    }
}
