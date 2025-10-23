using ElabdStor.Data;
using ElabdStor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class CategoryController : Controller
    {

        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        // عرض كل التصنيفات
        public IActionResult Index()
        {
            var categories = _db.Categories.ToList();
            return View(categories);
        }

        // صفحة إضافة جديدة
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "تم إضافة التصنيف بنجاح ✅";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // صفحة التعديل
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = _db.Categories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                TempData["success"] = "تم تعديل التصنيف بنجاح ✅";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // الحذف
        [HttpPost]
        public JsonResult DeleteAjax(int id)
        {
            var category = _db.Categories
                              .Include(c => c.Products) // للتأكد إذا هناك منتجات مرتبطة
                              .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "التصنيف غير موجود" });
            }

            if (category.Products.Any())
            {
                return Json(new { success = false, message = "لا يمكن حذف التصنيف لأنه يحتوي على منتجات" });
            }

            try
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
                return Json(new { success = true, message = "تم حذف التصنيف بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف: " + ex.Message });
            }
        }


    }
}
