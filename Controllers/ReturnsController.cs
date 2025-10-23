using ElabdStor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElabdStor.Controllers
{
    [RoleAuthorize("Admin")]
    public class ReturnsController : Controller
    {
        private readonly AppDbContext _db;

        public ReturnsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Return/Index
        // GET: /Returns/Index
        public IActionResult Index(DateTime? fromDate)
        {
            // جلب كل المرتجعات مع البيانات المرتبطة
            var returnsQuery = _db.Returns
                                  .Include(r => r.Product)
                                  .Include(r => r.Sale)
                                      .ThenInclude(s => s.Customer)
                                  .AsQueryable();

            // لو المستخدم أدخل تاريخ نفلتر المرتجعات من هذا التاريخ حتى اليوم
            if (fromDate.HasValue)
            {
                var toDate = DateTime.Now.Date.AddDays(1); // لحد اليوم الحالي
                returnsQuery = returnsQuery.Where(r => r.ReturnDate >= fromDate && r.ReturnDate < toDate);
            }

            var returns = returnsQuery
                          .OrderByDescending(r => r.ReturnDate)
                          .ToList();

            return View(returns);
        }
    }
}
