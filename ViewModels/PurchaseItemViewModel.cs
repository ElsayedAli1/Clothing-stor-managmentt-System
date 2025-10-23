namespace ElabdStor.ViewModels
{
    public class PurchaseItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal UnitCost { get; set; }
        public int Quantity { get; set; }
        public decimal ItemTotal => UnitCost * Quantity;

    }
}
