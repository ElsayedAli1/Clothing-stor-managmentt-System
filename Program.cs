using System;
using ElabdStor.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ElabdStor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // إضافة خدمات MVC
            builder.Services.AddControllersWithViews();

            // إعداد الاتصال بقاعدة البيانات
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("cs"))
           .ConfigureWarnings(warnings =>
               warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
              );
            // إضافة جلسة (Session)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8); // مدة بقاء الجلسة بدون نشاط
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // الوصول إلى HttpContext داخل أي Service مستقبلًا
            builder.Services.AddHttpContextAccessor();
            // ✅ إعداد حفظ المفاتيح في مجلد ثابت
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\ElabdKeys"))
                .SetApplicationName("ElabdStorSystem");

            var app = builder.Build();

            // إعدادات بيئة التشغيل
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // تفعيل الجلسة والصلاحيات
            app.UseSession();
           
            app.UseAuthorization();

            // المسار الافتراضي
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
