using System.ComponentModel.DataAnnotations;

namespace ElabdStor.ViewModels
{
    public class SaleCreateModel
    {
        public int? CustomerId { get; set; }

        [MaxLength(50)]
        public string? CustomerName { get; set; }

        [MaxLength(15)]
        public string? CustomerPhone { get; set; }

        // سنرسل العناصر كـ JSON string في حقل مخفي من الـ View
        [Required]
        public string ItemsJson { get; set; } = "";
    }
}
