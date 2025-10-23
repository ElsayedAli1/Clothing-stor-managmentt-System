using ElabdStor.Data;
using ElabdStor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElabdStor.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher;

        public AccountController(AppDbContext db)
        {
            _db = db;
            _hasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "من فضلك أدخل اسم المستخدم وكلمة المرور";
                return View();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            // حفظ بيانات الجلسة
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // صفحة عدم السماح
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // المستخدم لازم يكون مسجل دخول
            var uid = HttpContext.Session.GetInt32("UserId");
            if (!uid.HasValue) return RedirectToAction("Login");
            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            if (!uid.HasValue) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "كلمة المرور الجديدة غير مطابقة أو فارغة";
                return View();
            }

            var user = await _db.Users.FindAsync(uid.Value);
            if (user == null) return RedirectToAction("Login");

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verify == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "كلمة المرور الحالية خاطئة";
                return View();
            }

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            ViewBag.Message = "تم تغيير كلمة المرور بنجاح";
            return View();
        }
    }
}
