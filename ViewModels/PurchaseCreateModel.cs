using ElabdStor.Models;

namespace ElabdStor.ViewModels
{
    public class PurchaseCreateModel
    {
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public string ItemsJson { get; set; }
        public List<Category> Categories { get; set; }
    }
}
