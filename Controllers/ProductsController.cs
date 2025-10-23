using ElabdStor.Data;
using ElabdStor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;

        public ProductsController(AppDbContext db)
        {
            _db = db;
        }
        // لصفحة قائمة المنتجات
        public IActionResult Index()
        {
            return View();
        }

        // Action للبحث بالـAJAX
        public async Task<IActionResult> SearchByCode(string code)
        {
            var productsQuery = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(code))
                productsQuery = productsQuery.Where(p => p.Code.Contains(code));

            var products = await productsQuery
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    Category = p.Category != null ? p.Category.Name : "",
                    p.QuantityInStock,
                    p.PurchasePrice,
                    p.WholesalePrice,
                    p.HalfWholesalePrice,
                    p.RetailPrice
                })
                .ToListAsync();

            return Json(products);
        }

        // GET: /Products/GetByCode?code=...
        [HttpGet]
        [RoleAuthorize("Admin", "Employee")]
        public IActionResult GetByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return Json(new { success = false, message = "الرجاء إدخال كود المنتج" });

            code = code.Trim().ToUpper();
            var product = _db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Code.ToUpper() == code);

            if (product == null)
                return Json(new { success = false, message = "لم يتم العثور على المنتج" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = product.Id,
                    code = product.Code,
                    name = product.Name,
                    purchasePrice = product.PurchasePrice,
                    wholesalePrice = product.WholesalePrice,
                    halfWholesalePrice = product.HalfWholesalePrice,
                    retailPrice = product.RetailPrice,
                    quantityInStock = product.QuantityInStock,
                    categoryId = product.CategoryId,
                    categoryName = product.Category != null ? product.Category.Name : ""
                }
            });
        }

        public async Task<IActionResult> ByCategory(int categoryId)
        {
            // جلب كل المنتجات التي تنتمي لهذا التصنيف
            var products = await _db.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();

            return View(products); // نرسلها للـView
        }


        // صفحة إضافة منتج جديد
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Add(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // تعديل منتج
        public IActionResult Edit(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Update(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // حذف منتج (AJAX)
        // كنترولر Delete
        [HttpPost]
        [ValidateAntiForgeryToken] // لو عندك CSRF مفعّل
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            try
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "تم الحذف" });
            }
            catch (DbUpdateException)
            {
                // الخطأ عادة بسبب FK constraint
                return Json(new { success = false, message = "لا يمكن حذف المنتج لأنه مرتبط بسجلات أخرى." });
            }
            catch (Exception ex)
            {
                // أي خطأ آخر
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف: " + ex.Message });
            }
        }



        // عرض تفاصيل المنتج
        public IActionResult Details(int id)
        {
            var product = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
