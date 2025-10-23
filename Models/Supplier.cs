using System.ComponentModel.DataAnnotations;

namespace ElabdStor.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(50), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Address { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        // علاقة One-to-Many مع المشتريات
        public ICollection<Purchase> Purchases { get; set; }
    }
}
