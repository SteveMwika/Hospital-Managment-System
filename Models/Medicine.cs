using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Managment_System.Models
{
    public class Medicine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public decimal? Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public ICollection<MedicineInventoryLog> InventoryLogs { get; set; } = new List<MedicineInventoryLog>();
    }
}
