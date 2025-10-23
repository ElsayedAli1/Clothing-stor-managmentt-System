using System.ComponentModel.DataAnnotations;

namespace ElabdStor.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        // علاقة One-to-Many مع المنتجات
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
