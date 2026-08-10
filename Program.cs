using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

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
        context.SaveChanges(); // Lưu để Database tự sinh Id cho Category
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
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            },
            new Post 
            { 
                Title = "Razor View và Tag Helper", 
                Content = "Nội dung bài viết Razor View...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 10, 1),
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            },
            new Post 
            { 
                Title = "LINQ Thực hành", 
                Content = "Nội dung bài viết LINQ...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 9, 12),
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            },
            new Post 
            { 
                Title = "EF Core", 
                Content = "Nội dung bài viết EF Core...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 8, 5),
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            },
            new Post 
            { 
                Title = "MVC Nhập môn", 
                Content = "Nội dung bài viết MVC...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 7, 5),
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            },
            new Post 
            { 
                Title = "C# Cơ bản", 
                Content = "Nội dung bài viết C#...", 
                Author = "Admin", 
                PublishedAt = new DateTime(2024, 6, 5),
                CategoryId = defaultCategory.Id // 👈 ĐÃ BỔ SUNG KHÓA NGOẠI
            }
        );
        context.SaveChanges();
    }
}

app.Run();