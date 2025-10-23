namespace ElabdStor.ViewModels
{
    public class PurchaseItemJson
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }

        // الأسعار الأربعة اللي حضرتك طلبتهم
        public decimal PurchasePrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal HalfWholesalePrice { get; set; }
        public decimal RetailPrice { get; set; }

        public int CategoryId { get; set; }
    }
}
