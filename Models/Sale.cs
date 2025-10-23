using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElabdStor.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        // العلاقة مع العميل
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public decimal NetProfit { get; set; }//صافى الربح
        // علاقة One-to-Many مع تفاصيل البيع
        public ICollection<SaleItem> SaleItems { get; set; }
    }
}
