using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Models;
using Blogmanager_phamvanbinhminh.ViewModels;
using System.Text;
using System.Globalization;

namespace Blogmanager_phamvanbinhminh.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDBContext _context;

        public CategoriesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // 1. INDEX: Danh sách Danh mục + Tìm kiếm không dấu + Phân trang
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 5;

            // Nạp danh sách kèm đếm số Bài viết thuộc Danh mục
            var categoriesList = await _context.Categories
                .Include(c => c.Posts)
                .ToListAsync();

            // Tìm kiếm tiếng Việt (có dấu & không dấu)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchNoSign = RemoveDiacritics(search.Trim().ToLower());
                var keywords = searchNoSign.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                categoriesList = categoriesList.Where(c =>
                {
                    var nameNoSign = RemoveDiacritics((c.Name ?? "").ToLower());
                    return keywords.All(k => nameNoSign.Contains(k));
                }).ToList();
            }

            // Sắp xếp theo tên A-Z
            var query = categoriesList.OrderBy(c => c.Name).AsQueryable();

            // Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;

            var pagedCategories = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            CategoryListViewModel model = new CategoryListViewModel
            {
                Categories = pagedCategories,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };

            return View(model);
        }

        // 2. DETAILS: Xem chi tiết danh mục (kèm danh sách bài viết thuộc danh mục)
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();
            return View(category);
        }

        // 3. CREATE: Hiển thị form và xử lý Thêm danh mục
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 4. EDIT: Hiển thị form và xử lý Sửa danh mục
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();
            if (!ModelState.IsValid) return View(category);

            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 5. DELETE: Hiển thị trang xác nhận và xử lý Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Hàm hỗ trợ xóa dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString()
                                .Normalize(NormalizationForm.FormC)
                                .Replace('đ', 'd')
                                .Replace('Đ', 'D');
        }
    }
}