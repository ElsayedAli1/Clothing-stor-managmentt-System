using ElabdStor.Data;
using ElabdStor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // Index: يعرض لستة المصروفات، يدعم فلترة بالتاريخ ويعرض الإجمالي للفترة
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Expenses.AsQueryable();

                if (startDate.HasValue)
                {
                    var sd = startDate.Value.Date;
                    query = query.Where(e => e.CreatedAt >= sd);
                }

                if (endDate.HasValue)
                {
                    var ed = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(e => e.CreatedAt <= ed);
                }

                var list = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                // حساب الإجمالي (لو مفيش صفوف نرجع 0)
                var total = list.Any() ? list.Sum(e => e.Amount) : 0m;

                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                ViewBag.Total = total;

                // رسائل من عمليات سابقة (نجاح/خطأ)
                ViewBag.SuccessMessage = TempData["Success"] as string;
                ViewBag.ErrorMessage = TempData["Error"] as string;

                return View(list);
            }
            catch (Exception ex)
            {
                // سجل أو غيّر التصرف حسب مشروعك
                TempData["Error"] = "حدث خطأ أثناء جلب المصروفات: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Create GET
        public IActionResult Create()
        {
            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Category,Amount,Description")] Expense expense)
        {
            if (!ModelState.IsValid)
            {
                return View(expense);
            }

            try
            {
                // تأكيد تاريخ الإنشاء من الكود (آمن للعمل أوفلاين ولتجنب SqlNull issues)
                expense.CreatedAt = DateTime.Now;

                _context.Add(expense);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة المصروف بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                // خطأ في حفظ البيانات
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ المصروف. تأكد من القيم وحاول مرة أخرى.");
                TempData["Error"] = "خطأ في حفظ المصروف: " + dbEx.Message;
                return View(expense);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ غير متوقع.");
                TempData["Error"] = "خطأ غير متوقع: " + ex.Message;
                return View(expense);
            }
        }

        // Edit GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id.Value);
            if (expense == null) return NotFound();

            return View(expense);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Category,Amount,Description")] Expense expense)
        {
            if (id != expense.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(expense);
            }

            try
            {
                var exist = await _context.Expenses.FindAsync(id);
                if (exist == null) return NotFound();

                // لا نغير CreatedAt هنا
                exist.Category = expense.Category;
                exist.Amount = expense.Amount;
                exist.Description = expense.Description;

                _context.Update(exist);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تحديث المصروف بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(expense.Id))
                {
                    return NotFound();
                }
                else
                {
                    TempData["Error"] = "تعارض أثناء حفظ التعديلات. حاول مرة أخرى.";
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء التحديث: " + ex.Message;
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء التحديث.");
                return View(expense);
            }
        }

        // Details (Partial view — مناسب لجلب AJAX وعرضه في modal)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (expense == null) return NotFound();

            return PartialView("_DetailsPartial", expense);
        }
        // عرض التقرير
        public IActionResult Report(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Expenses.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.CreatedAt <= toDate.Value);

            var expenses = query.OrderByDescending(e => e.CreatedAt).ToList();

            ViewBag.Total = expenses.Sum(e => e.Amount);
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return Json(new { success = false, message = "المصروف غير موجود." });

            try
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "تم حذف المصروف بنجاح." });
            }
            catch
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        private bool ExpenseExists(int id) =>
            _context.Expenses.Any(e => e.Id == id);
    }
}
