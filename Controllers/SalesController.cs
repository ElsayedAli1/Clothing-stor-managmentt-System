using System.Text.Json;
using ElabdStor.Data;
using ElabdStor.Models;
using ElabdStor.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElabdStor.Controllers
{
    
    public class SalesController : Controller
    {
        private readonly AppDbContext _db;

        public SalesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: صفحة إنشاء فاتورة جديدة
        [RoleAuthorize("Admin", "Employee")]
        public IActionResult Create()
        {
            ViewBag.Customers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_db.Customers.ToList(), "Id", "Name");
            return View();
        }

        // GET: API لجلب المنتج بالكود
        [HttpGet]
        [RoleAuthorize("Admin", "Employee")]
        public IActionResult GetProductByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "الكود مطلوب" });

            var product = _db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Code == code);

            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            return Json(new
            {
                success = true,
                data = new
                {
                    product.Id,
                    product.Code,
                    product.Name,
                    product.PurchasePrice,
                    product.WholesalePrice,
                    product.HalfWholesalePrice,
                    product.RetailPrice,
                    product.QuantityInStock,
                    CategoryName = product.Category?.Name
                }
            });
        }

        // POST: استلام وحفظ الفاتورة
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Employee")]
        public IActionResult Create(SaleCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "بيانات الفاتورة غير صحيحة";
                return RedirectToAction("Create");
            }

            List<SaleItemDto> items;
            try
            {
                items = JsonSerializer.Deserialize<List<SaleItemDto>>(model.ItemsJson) ?? new List<SaleItemDto>();
            }
            catch
            {
                TempData["error"] = "خطأ في بيانات المنتجات";
                return RedirectToAction("Create");
            }

            if (items.Count == 0)
            {
                TempData["error"] = "لا يوجد منتجات في الفاتورة";
                return RedirectToAction("Create");
            }

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                // ✅ البحث عن العميل برقم الهاتف أولًا
                Customer customer = null;
                if (!string.IsNullOrWhiteSpace(model.CustomerPhone))
                {
                    customer = _db.Customers.FirstOrDefault(c => c.Phone == model.CustomerPhone);
                }

                // ✅ لو العميل مش موجود، إنشئه جديد
                if (customer == null)
                {
                    if (string.IsNullOrWhiteSpace(model.CustomerName))
                    {
                        TempData["error"] = "من فضلك أدخل اسم العميل.";
                        return RedirectToAction("Create");
                    }

                    customer = new Customer
                    {
                        Name = model.CustomerName,
                        Phone = model.CustomerPhone,
                        TotalPurchases = 0 // مبدئيًا
                    };
                    _db.Customers.Add(customer);
                    _db.SaveChanges();
                }

                // ✅ إنشاء الفاتورة
                var sale = new Sale
                {
                    SaleDate = DateTime.Now,
                    CustomerId = customer.Id,
                    NetProfit = 0
                };
                _db.Sales.Add(sale);
                _db.SaveChanges();

                decimal totalProfit = 0;
                decimal totalSaleAmount = 0;

                // ✅ حفظ المنتجات في الفاتورة
                foreach (var it in items)
                {
                    var product = _db.Products.FirstOrDefault(p => p.Id == it.ProductId);
                    if (product == null)
                    {
                        transaction.Rollback();
                        TempData["error"] = $"المنتج (كود: {it.ProductCode}) غير موجود.";
                        return RedirectToAction("Create");
                    }

                    if (product.QuantityInStock < it.Quantity)
                    {
                        transaction.Rollback();
                        TempData["error"] = $"كمية المنتج \"{product.Name}\" غير كافية.";
                        return RedirectToAction("Create");
                    }

                    decimal itemProfit = (it.UnitPrice - product.PurchasePrice) * it.Quantity;
                    totalProfit += itemProfit;
                    totalSaleAmount += it.UnitPrice * it.Quantity;

                    product.QuantityInStock -= it.Quantity;
                    _db.Products.Update(product);

                    var saleItem = new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = product.Id,
                        Quantity = it.Quantity,
                        UnitPrice = it.UnitPrice
                    };
                    _db.SaleItems.Add(saleItem);
                }

                // ✅ تحديث صافي الربح
                sale.NetProfit = totalProfit;
                _db.Sales.Update(sale);

                // ✅ تحديث إجمالي مشتريات العميل
                customer.TotalPurchases += totalSaleAmount;
                _db.Customers.Update(customer);

                _db.SaveChanges();
                transaction.Commit();

                TempData["success"] = "تم حفظ الفاتورة بنجاح.";
                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["error"] = "حدث خطأ أثناء الحفظ: " + ex.Message;
                return RedirectToAction("Create");
            }
        }




        // (اختياري) صفحة عرض كل الفواتير
        [RoleAuthorize("Admin")]
        public IActionResult Index()
        {
            var sales = _db.Sales
                .Include(s => s.Customer) // جلب بيانات العميل
                .Include(s => s.SaleItems) // جلب عناصر الفاتورة
                    .ThenInclude(si => si.Product) // جلب بيانات المنتج لكل عنصر
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            return View(sales);
        }



        // تفاصيل فاتورة

        [RoleAuthorize("Admin")]
        public IActionResult Details(int id)
        {
            var sale = _db.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null) return NotFound();
            return View(sale);
        }
        // عرض الصفحة
        [RoleAuthorize("Admin")]
        public IActionResult Report()
        {
            return View();
        }

        // استقبال التاريخ وجلب النتائج
        [HttpPost]
        [RoleAuthorize("Admin")]
        public IActionResult Report(DateTime fromDate)
        {
            // نجيب المبيعات من التاريخ لحد النهارده
            var sales = _db.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.SaleDate >= fromDate && s.SaleDate <= DateTime.Now)
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.TotalSales = sales.Sum(s => s.SaleItems.Sum(i => i.UnitPrice * i.Quantity));
            ViewBag.TotalProfit = sales.Sum(s => s.NetProfit);

            return View(sales);
        }
        // المرتجع 
        [HttpPost]
        // [ValidateAntiForgeryToken] // إن أردت حماية, لكن إذا تستقبل JSON عبر body يمكنك استخدام [FromBody] وارسال التوكن هيدر
        [RoleAuthorize("Admin", "Employee")]
        public IActionResult ReturnProduct([FromBody] ReturnRequest req)
        {
            if (req == null || req.Quantity <= 0)
                return Json(new { success = false, message = "بيانات غير صحيحة" });

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var saleItem = _db.SaleItems
                    .Include(si => si.Product)
                    .Include(si => si.Sale)
                    .FirstOrDefault(si => si.SaleId == req.SaleId && si.ProductId == req.ProductId);

                if (saleItem == null)
                    return Json(new { success = false, message = "المنتج غير موجود في الفاتورة" });

                if (req.Quantity > saleItem.Quantity)
                    return Json(new { success = false, message = "الكمية المراد إرجاعها أكبر من المباعة" });

                var product = saleItem.Product;
                var sale = saleItem.Sale;

                // تحديث الكمية في المخزن
                product.QuantityInStock += req.Quantity;
                _db.Products.Update(product);

                // تحديث SaleItem (طرح الكمية أو حذفه إذا صفر)
                saleItem.Quantity -= req.Quantity;
                if (saleItem.Quantity <= 0)
                    _db.SaleItems.Remove(saleItem);
                else
                    _db.SaleItems.Update(saleItem);

                // مبلغ المرتجع
                decimal refundAmount = req.Quantity * saleItem.UnitPrice;

                // سجل المرتجع
                var returnRecord = new Return
                {
                    SaleId = req.SaleId,
                    ProductId = req.ProductId,
                    Quantity = req.Quantity,
                    ReturnDate = DateTime.Now,
                    RefundAmount = refundAmount
                };
                _db.Returns.Add(returnRecord);

                // حفظ مؤقت
                _db.SaveChanges();

                // إعادة حساب صافي الربح وإجمالي الفاتورة
                var updatedSaleItems = _db.SaleItems.Where(si => si.SaleId == sale.Id).ToList();
                decimal newNetProfit = 0m;
                foreach (var it in updatedSaleItems)
                {
                    var prod = _db.Products.FirstOrDefault(p => p.Id == it.ProductId);
                    if (prod != null)
                    {
                        newNetProfit += (it.UnitPrice - prod.PurchasePrice) * it.Quantity;
                    }
                }
                sale.NetProfit = newNetProfit;
                _db.Sales.Update(sale);

                // تحديث إجمالي مشتريات العميل إذا تستخدم حقل ثابت
                var customer = _db.Customers.FirstOrDefault(c => c.Id == sale.CustomerId);
                if (customer != null)
                {
                    customer.TotalPurchases -= refundAmount;
                    if (customer.TotalPurchases < 0) customer.TotalPurchases = 0;
                    _db.Customers.Update(customer);
                }

                _db.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, message = "تمت عملية المرتجع بنجاح", refund = refundAmount });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = ex.ToString() });
            }

        }



    }
}
