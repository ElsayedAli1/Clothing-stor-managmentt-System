namespace ElabdStor.ViewModels
{
    public class CustomerSaleDto
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CustomerSaleItemDto> Items { get; set; }
    }
}
