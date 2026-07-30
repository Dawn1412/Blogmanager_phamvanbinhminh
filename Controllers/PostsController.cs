using Microsoft.AspNetCore.Mvc;
using Blogmanager_phamvanbinhminh.Models;

namespace Blogmanager_phamvanbinhminh.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;

        public PostsController(ILogger<PostsController> logger)
        {
            _logger = logger;
        }

        private List<Post> GetPosts()
        {
            return new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "C# cơ bản",
                    Author = "Nguyễn Văn An",
                    Content = "Bài viết giới thiệu các khái niệm nền tảng của C#: biến, kiểu dữ liệu, vòng lặp và câu lệnh điều kiện.",
                    PublishedAt = new DateTime(2026, 6, 5),
                    IsPublished = true,
                    ViewCount = 150
                },
                new Post
                {
                    Id = 2,
                    Title = "MVC nhập môn",
                    Author = "Phạm Thu Hà",
                    Content = "Tìm hiểu mô hình MVC trong ASP.NET Core: vai trò của Model, View, Controller và cách chúng phối hợp xử lý request.",
                    PublishedAt = new DateTime(2026, 7, 5),
                    IsPublished = true,
                    ViewCount = 300
                },
                new Post
                {
                    Id = 3,
                    Title = "EF Core",
                    Author = "Lê Hoàng Nam",
                    Content = "Giới thiệu Entity Framework Core: DbContext, migration và cách truy vấn dữ liệu bằng LINQ.",
                    PublishedAt = new DateTime(2026, 8, 5),
                    IsPublished = false,
                    ViewCount = 200
                }
            };
        }

        public IActionResult Index()
        {
            var posts = GetPosts();
            return View(posts);
        }

        public IActionResult Details(int id)
        {
            var post = GetPosts().FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}