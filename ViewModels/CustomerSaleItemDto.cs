namespace ElabdStor.ViewModels
{
    public class CustomerSaleItemDto
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ItemTotal { get; set; }
        public int ProductId { get; set; }
    }
}
