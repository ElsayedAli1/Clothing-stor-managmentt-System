namespace ElabdStor.ViewModels
{
    public class SupplierInvoiceViewModel
    {
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
