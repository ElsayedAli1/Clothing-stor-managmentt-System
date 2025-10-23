using System.ComponentModel.DataAnnotations;

namespace ElabdStor.Models
{
    public class Return
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale Sale { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public DateTime ReturnDate { get; set; }

        public decimal RefundAmount { get; set; }
    }
}
