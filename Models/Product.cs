using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElabdStor.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required, MaxLength(30)]
        public string Code { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }   // سعر الشراء

        [Column(TypeName = "decimal(18,2)")]
        public decimal WholesalePrice { get; set; }  // سعر الجملة

        [Column(TypeName = "decimal(18,2)")]
        public decimal HalfWholesalePrice { get; set; } // نص الجملة

        [Column(TypeName = "decimal(18,2)")]
        public decimal RetailPrice { get; set; }     // سعر التجزئة

        public int QuantityInStock { get; set; }

        // العلاقة مع الكاتيجوري
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
