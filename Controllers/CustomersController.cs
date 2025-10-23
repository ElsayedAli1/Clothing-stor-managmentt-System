using ElabdStor.Data;
using Microsoft.AspNetCore.Mvc;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin", "Employee")]
    public class CustomersController : Controller
    {
        public readonly AppDbContext _db;
        public CustomersController(AppDbContext db)
        {
            _db = db;
        }
       
        [HttpGet]
        public IActionResult GetByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { success = false, message = "رقم الهاتف غير صالح" });

            var customer = _db.Customers.FirstOrDefault(c => c.Phone == phone);

            if (customer == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = customer.Id,
                    name = customer.Name
                }
            });
        }
        // GET: /Customer
        public IActionResult Index(string? searchName)
        {
            // جلب العملاء مع حساب إجمالي المشتريات لكل واحد (محسوب من SaleItems)
            var customersQuery = _db.Customers
                .Select(c => new
                {
                    Customer = c,
                    Total = _db.Sales
                                .Where(s => s.CustomerId == c.Id)
                                .SelectMany(s => s.SaleItems)
                                .Sum(si => (decimal?)si.Quantity * si.UnitPrice) ?? 0m
                });

            // لو المستخدم كتب اسم نفلتر قبل التحويل للـ ViewModel
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                customersQuery = customersQuery.Where(x => x.Customer.Name.Contains(searchName));
            }

            var customers = customersQuery
                .AsEnumerable() // رجع لــ in-memory لكي نبني ViewModel
                .Select(x => new ElabdStor.ViewModels.CustomerSummaryViewModel
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Name,
                    Phone = x.Customer.Phone,
                    TotalPurchases = x.Total
                })
                .OrderByDescending(c => c.TotalPurchases)
                .ToList();

            return View(customers);
        }


        // GET: /Customer/Details/5
        public IActionResult Details(int id)
        {
            var customer = _db.Customers
                .Where(c => c.Id == id)
                .Select(c => new ElabdStor.ViewModels.CustomerDetailsViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    Sales = c.Sales
                        .OrderByDescending(s => s.SaleDate)
                        .Select(s => new ElabdStor.ViewModels.CustomerSaleDto
                        {
                            SaleId = s.Id,
                            SaleDate = s.SaleDate,
                            TotalAmount = s.SaleItems.Sum(si => (decimal?)si.Quantity * si.UnitPrice) ?? 0m,
                            Items = s.SaleItems.Select(si => new ElabdStor.ViewModels.CustomerSaleItemDto
                            {
                                ProductId = si.ProductId,
                                ProductCode = si.Product.Code,
                                ProductName = si.Product.Name,
                                UnitPrice = si.UnitPrice,
                                Quantity = si.Quantity,
                                ItemTotal = si.UnitPrice * si.Quantity
                            }).ToList()
                        }).ToList()
                })
                .FirstOrDefault();

            if (customer == null) return NotFound();
            return View(customer);
        }


    }
}
