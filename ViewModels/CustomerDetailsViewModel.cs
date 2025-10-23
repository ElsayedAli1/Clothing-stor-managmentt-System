namespace ElabdStor.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public List<CustomerSaleDto> Sales { get; set; } = new List<CustomerSaleDto>();
    }
}