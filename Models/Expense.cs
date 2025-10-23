using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElabdStor.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Category { get; set; } = string.Empty;    // بند الصرف

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;    // التكلفة

        public string? Description { get; set; }    // وصف

        // نخلي التاريخ يتضاف من الكود بدل ما نعتمد على SQL
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}