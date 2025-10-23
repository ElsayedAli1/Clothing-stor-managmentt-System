namespace ElabdStor.ViewModels
{
    public class PurchaseDetailsViewModel
    {
        public int PurchaseId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PurchaseItemDetalisViewModel> Items { get; set; }
    }
}
