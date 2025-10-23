namespace ElabdStor.ViewModels
{
    public class SupplierDetailsViewModel
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public List<SupplierPurchaseViewModel> Purchases { get; set; }

    }
}
