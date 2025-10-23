namespace ElabdStor.ViewModels
{
    public class SupplierPurchaseViewModel
    {
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        public List<PurchaseItemViewModel> Items { get; set; }
    }
}
