using System;
using System.Collections.Generic;
using System.Linq;
using ElabdStor.Data;
using ElabdStor.Models;
using ElabdStor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class PurchasesController : Controller
    {
        private readonly AppDbContext _db;

        public PurchasesController(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var purchases = await _db.Purchases
                .Include(p => p.Supplier)
                .Select(p => new PurchaseSummaryViewModel
                {
                    PurchaseId = p.Id,
                    SupplierName = p.Supplier.Name,
                    SupplierPhone = p.Supplier.Phone,
                    TotalAmount = p.TotalAmount
                })
                .ToListAsync();

            return View(purchases);
        }
        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // جلب الفاتورة مع المورد و PurchaseItems و Product لكل Item
            var purchase = await _db.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return NotFound();

            var model = new PurchaseDetailsViewModel
            {
                PurchaseId = purchase.Id,
                SupplierName = purchase.Supplier.Name,
                SupplierPhone = purchase.Supplier.Phone,
                TotalAmount = purchase.TotalAmount,
                Items = purchase.PurchaseItems.Select(pi => new PurchaseItemDetalisViewModel
                {
                    ProductCode = pi.Product.Code,
                    ProductName = pi.Product.Name,
                    UnitPrice = pi.UnitCost,
                    Quantity = pi.Quantity,
                    ItemTotal = pi.UnitCost * pi.Quantity
                }).ToList()
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult Create()
        {
            var model = new PurchaseCreateModel
            {
                Categories = _db.Categories.OrderBy(c => c.Name).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PurchaseCreateModel model)
        {
            // إعادة تعبئة الكاتيجوريات لتجنب NullReferenceException
            model.Categories = _db.Categories.OrderBy(c => c.Name).ToList();

            if (string.IsNullOrEmpty(model.SupplierPhone))
            {
                TempData["error"] = "يجب إدخال رقم المورد.";
                return View(model);
            }

            // البحث عن المورد برقم الهاتف
            var supplier = _db.Suppliers.FirstOrDefault(s => s.Phone == model.SupplierPhone);

            if (supplier == null)
            {
                // مورد جديد
                supplier = new Supplier
                {
                    Name = model.SupplierName,
                    Phone = model.SupplierPhone,
                    Email = model.SupplierEmail,
                    Address = model.SupplierAddress,
                    LastPurchaseDate = DateTime.Now
                };
                _db.Suppliers.Add(supplier);
                try
                {
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }

            }
            else
            {
                // مورد موجود → تحديث بياناته (لو الاسم أو العنوان أو الإيميل اتغير)
                supplier.Name = string.IsNullOrEmpty(model.SupplierName) ? supplier.Name : model.SupplierName;
                supplier.Email = model.SupplierEmail ?? supplier.Email;
                supplier.Address = model.SupplierAddress ?? supplier.Address;
                supplier.LastPurchaseDate = DateTime.Now;

                _db.Suppliers.Update(supplier);
                _db.SaveChanges();
            }

            // إنشاء الفاتورة
            // قراءة الأصناف من JSON
            var items = JsonConvert.DeserializeObject<List<PurchaseItemJson>>(model.ItemsJson);

            // حساب الإجمالي
            decimal totalAmount = items.Sum(i => i.Quantity * i.PurchasePrice);

            // المبلغ المدفوع يأخذ مباشرة من الموديل
            decimal paidAmount = model.PaidAmount;

            // المبلغ المتبقي يُحسب من الفرق بين الإجمالي والمدفوع
            decimal remainingAmount = totalAmount - paidAmount;

            // إنشاء الفاتورة
            var purchase = new Purchase
            {
                SupplierId = supplier.Id,
                PurchaseDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = remainingAmount
            };

            _db.Purchases.Add(purchase);
            _db.SaveChanges();


            // قراءة الأصناف من JSON
          

            foreach (var itemJson in items)
            {
                var product = _db.Products.FirstOrDefault(p => p.Code == itemJson.Code);

                if (product != null)
                {
                    // تحديث المنتج الموجود
                    product.QuantityInStock += itemJson.Quantity;
                    product.PurchasePrice = itemJson.PurchasePrice;
                    product.WholesalePrice = itemJson.WholesalePrice;
                    product.HalfWholesalePrice = itemJson.HalfWholesalePrice;
                    product.RetailPrice = itemJson.RetailPrice;
                    product.CategoryId = itemJson.CategoryId;

                    _db.Products.Update(product);
                }
                else
                {
                    // إضافة منتج جديد
                    product = new Product
                    {
                        Code = itemJson.Code,
                        Name = itemJson.Name,
                        QuantityInStock = itemJson.Quantity,
                        PurchasePrice = itemJson.PurchasePrice,
                        WholesalePrice = itemJson.WholesalePrice,
                        HalfWholesalePrice = itemJson.HalfWholesalePrice,
                        RetailPrice = itemJson.RetailPrice,
                        CategoryId = itemJson.CategoryId
                    };
                    _db.Products.Add(product);
                    _db.SaveChanges();
                }

                // إضافة تفاصيل الصنف فى الفاتورة
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = product.Id,
                    Quantity = itemJson.Quantity,
                    UnitCost = itemJson.PurchasePrice
                };
                _db.PurchaseItems.Add(purchaseItem);
            }

            _db.SaveChanges();
            TempData["success"] = "تم حفظ فاتورة الشراء بنجاح.";
            return RedirectToAction("Index");
        }

        private class PurchaseItemJson
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal PurchasePrice { get; set; }
            public decimal WholesalePrice { get; set; }
            public decimal HalfWholesalePrice { get; set; }
            public decimal RetailPrice { get; set; }
            public int CategoryId { get; set; }
        }
    }
}
