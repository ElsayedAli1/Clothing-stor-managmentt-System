using System.ComponentModel.DataAnnotations;

namespace ElabdStor.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(50), EmailAddress]
        public string? Email { get; set; }
        public decimal? TotalPurchases { get; set; }

        // علاقة One-to-Many مع المبيعات
        public ICollection<Sale> Sales { get; set; }
    }
}
