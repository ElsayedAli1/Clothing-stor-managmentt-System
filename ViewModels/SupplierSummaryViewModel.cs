namespace ElabdStor.ViewModels
{
    public class SupplierSummaryViewModel
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public decimal TotalPurchasesAmount { get; set; } // إجمالي المشتريات من المورد
        public decimal TotalPaid { get; set; }           // إجمالي المدفوع
        public decimal TotalRemaining => TotalPurchasesAmount - TotalPaid; // الباقي عليه
    }
}
