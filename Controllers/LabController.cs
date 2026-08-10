using Blogmanager_phamvanbinhminh.Models;
using Microsoft.AspNetCore.Mvc;

public class LabController : Controller
{
    public IActionResult Index()
    {
        var baiViet = new List<Post>
        {
            new Post { Id = 1, Title = "C# cơ bản", Author = " Minh", ViewCount = 100, IsPublished = true },
            new Post { Id = 2, Title = "MVC nhập môn", Author = "Hoàng", ViewCount = 50, IsPublished = false },
            new Post { Id = 3, Title = "EF Core", Author = "Tuyên", ViewCount = 200, IsPublished = true },
            new Post { Id = 4, Title = "Lập trình ứng dụng Web", Author = "Đạt", ViewCount = 150, IsPublished = true },
            new Post { Id = 5, Title = "Git & GitHub", Author = "Quân", ViewCount = 75, IsPublished = false }
        };

        ViewBag.SoDaXuatBan = baiViet.Count(p => p.IsPublished);
        ViewBag.TieuDe = baiViet.Where(p => p.IsPublished)
            .OrderBy(p => p.Title).Select(p => p.Title).ToList();

        ViewBag.BaiDaXuatBan = baiViet.Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount).ToList();

        ViewBag.TongLuotXem = baiViet.Where(p => p.IsPublished).Sum(p => p.ViewCount);

        ViewBag.BaiVietHotNhat = baiViet.OrderByDescending(p => p.ViewCount).FirstOrDefault();
        return View();
    }
}