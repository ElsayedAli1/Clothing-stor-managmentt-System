using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElabdStor.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // العلاقة مع المورد
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        // المبلغ الإجمالي
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // المبلغ المدفوع
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        // المبلغ المتبقي
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }
        // علاقة One-to-Many مع تفاصيل المشتريات
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
    }
}
