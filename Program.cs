using Microsoft.AspNetCore.Identity;
using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"]!;
        options.ClientSecret = googleAuthNSection["ClientSecret"]!;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting(); // 1. Định tuyến trước

// 🔥 2. Bổ sung Middleware Xác thực & Phân quyền chuẩn thứ tự
app.UseAuthentication(); // 👈 BỔ SUNG: Kiểm tra "Bạn là ai?"
app.UseAuthorization();  // Kiểm tra "Bạn được làm gì?"

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 🔥 3. Bổ sung MapRazorPages cho giao diện Identity UI
app.MapRazorPages(); // 👈 BỔ SUNG: Cho phép chạy trang Đăng ký/Đăng nhập

// ========================================================
// SEED DATA (TẠO DỮ LIỆU MẪU KHI CHẠY APP)
// ========================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    
    // 1. Tạo Danh mục mẫu trước (nếu chưa có)
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Lập trình Web" },
            new Category { Name = "Kiến thức C#" }
        );
        context.SaveChanges();
    }

    // Lấy Id của danh mục vừa tạo để gán cho các bài viết bên dưới
    var defaultCategory = context.Categories.First();

    // 2. Tạo Bài viết mẫu và gán CategoryId hợp lệ (nếu chưa có bài viết)
    if (!context.Posts.Any())
    {
        context.Posts.AddRange(
            new Post 
            { 
                Title = "Triển khai ứng dụng ASP.NET Core", 
                Content = "Nội dung bài viết ASP.NET Core...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 1, 20),
                CategoryId = defaultCategory.Id 
            },
            new Post 
            { 
                Title = "Razor View và Tag Helper", 
                Content = "Nội dung bài viết Razor View...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 10, 1),
                CategoryId = defaultCategory.Id 
            },
            new Post 
            { 
                Title = "LINQ Thực hành", 
                Content = "Nội dung bài viết LINQ...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 9, 12),
                CategoryId = defaultCategory.Id 
            },
            new Post 
            { 
                Title = "EF Core", 
                Content = "Nội dung bài viết EF Core...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 8, 5),
                CategoryId = defaultCategory.Id 
            },
            new Post 
            { 
                Title = "MVC Nhập môn", 
                Content = "Nội dung bài viết MVC...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 7, 5),
                CategoryId = defaultCategory.Id 
            },
            new Post 
            { 
                Title = "C# Cơ bản", 
                Content = "Nội dung bài viết C#...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 6, 5),
                CategoryId = defaultCategory.Id 
            }
        );
        context.SaveChanges();
    }
}
// --- ĐOẠN SEED ROLE VÀ TÀI KHOẢN ADMIN ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Tạo các Role mặc định nếu chưa có
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2. Tạo tài khoản Admin mặc định
    var email = "admin@blogmanager.local";
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var admin = new IdentityUser 
        { 
            UserName = email, 
            Email = email, 
            EmailConfirmed = true 
        };

        // Mật khẩu Admin: Admin@123
        var result = await userManager.CreateAsync(admin, "Admin@123");
        
        if (result.Succeeded)
        {
            // Gán Role "Admin" cho tài khoản này
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
// ------------------------------------------

app.Run();