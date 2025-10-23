using ElabdStor.Data;
using ElabdStor.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _db;
        public SuppliersController (AppDbContext db)
        {
            _db = db;
        }
        // جدول الموردين
        public IActionResult Index()
        {
            var suppliers = _db.Suppliers
                .Select(s => new SupplierSummaryViewModel
                {
                    SupplierId = s.Id,
                    Name = s.Name,
                    Phone = s.Phone,
                    TotalPurchasesAmount = s.Purchases.Sum(p => p.TotalAmount),
                    TotalPaid = s.Purchases.Sum(p => p.PaidAmount)
                })
                .ToList();

            return View(suppliers);
        }

        // تفاصيل المورد مع جميع الفواتير
        public IActionResult Details(int id)
        {
            var supplier = _db.Suppliers
                .Where(s => s.Id == id)
                .Select(s => new SupplierDetailsViewModel
                {
                    SupplierId = s.Id,
                    Name = s.Name,
                    Phone = s.Phone,
                    Purchases = s.Purchases
                        .OrderByDescending(p => p.PurchaseDate)
                        .Select(p => new SupplierPurchaseViewModel
                        {
                            PurchaseId = p.Id,
                            PurchaseDate = p.PurchaseDate,
                            TotalAmount = p.TotalAmount,
                            PaidAmount = p.PaidAmount,
                            Items = p.PurchaseItems
                                .Select(pi => new PurchaseItemViewModel
                                {
                                    ProductId = pi.ProductId,
                                    ProductCode = pi.Product.Code,
                                    ProductName = pi.Product.Name,
                                    UnitCost = pi.UnitCost,
                                    Quantity = pi.Quantity
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

      
        [HttpGet]
        public IActionResult GetByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { success = false, message = "رقم الهاتف فارغ" });

            var s = _db.Suppliers.FirstOrDefault(x => x.Phone == phone);
            if (s == null)
                return Json(new { success = false, data = (object)null });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = s.Id,
                    name = s.Name,
                    email = s.Email,
                    address = s.Address
                }
            });
        }


       
    }
}
