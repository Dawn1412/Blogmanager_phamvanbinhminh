using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Models;
using Blogmanager_phamvanbinhminh.ViewModels;
using System.Text;
using System.Globalization;

namespace Blogmanager_phamvanbinhminh.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDBContext _context;

        public PostsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // 1. INDEX: Danh sách bài viết + Tìm kiếm tiếng Việt + Phân trang
        public async Task<IActionResult> Index(string? search, string? sort, int page = 1)
        {
            int pageSize = 5;

            var postsQuery = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .ToListAsync();

            // Tìm kiếm tiếng Việt (có dấu & không dấu)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchNoSign = RemoveDiacritics(search.Trim().ToLower());
                var keywords = searchNoSign.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                postsQuery = postsQuery.Where(p =>
                {
                    var titleNoSign = RemoveDiacritics((p.Title ?? "").ToLower());
                    var contentNoSign = RemoveDiacritics((p.Content ?? "").ToLower());

                    return keywords.All(k => titleNoSign.Contains(k) || contentNoSign.Contains(k));
                }).ToList();
            }

            // Sắp xếp
            var query = postsQuery.AsQueryable();
            query = sort switch
            {
                "title" => query.OrderBy(p => p.Title),
                "oldest" => query.OrderBy(p => p.PublishedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;

            var pagedPosts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            PostListViewModel model = new PostListViewModel
            {
                Posts = pagedPosts,
                Search = search,
                Sort = sort,
                CurrentPage = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };

            return View(model);
        }

        // 2. DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // 3. CREATE
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, int[]? selectedTagIds, string? newTags)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.SelectedTagIds = selectedTagIds?.ToList() ?? new List<int>();
                ViewBag.NewTags = newTags;
                return View(post);
            }

            // 1. Xử lý gán thẻ có sẵn
            if (selectedTagIds != null && selectedTagIds.Length > 0)
            {
                var existingTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
                post.Tags.AddRange(existingTags);
            }

            // 2. Xử lý thẻ mới từ Input text
            if (!string.IsNullOrWhiteSpace(newTags))
            {
                var tagNames = newTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrEmpty(t))
                                      .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var tagName in tagNames)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (existingTag != null)
                    {
                        if (!post.Tags.Any(t => t.Id == existingTag.Id))
                        {
                            post.Tags.Add(existingTag);
                        }
                    }
                    else
                    {
                        post.Tags.Add(new Tag { Name = tagName });
                    }
                }
            }

            post.CreatedAt = DateTime.Now;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 4. EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();
            ViewBag.SelectedTagIds = post.Tags.Select(t => t.Id).ToList();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post, int[]? selectedTagIds, string? newTags)
        {
            if (id != post.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.SelectedTagIds = selectedTagIds?.ToList() ?? new List<int>();
                ViewBag.NewTags = newTags;
                return View(post);
            }

            try
            {
                var existingPost = await _context.Posts
                    .Include(p => p.Tags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingPost == null) return NotFound();

                // Cập nhật thông tin cơ bản
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.Author = post.Author;
                existingPost.PublishedAt = post.PublishedAt;
                existingPost.IsPublished = post.IsPublished;
                existingPost.CategoryId = post.CategoryId;

                // Cập nhật Tags: Xóa toàn bộ tag cũ
                existingPost.Tags.Clear();

                // Gán các tag đã chọn từ Checkbox
                if (selectedTagIds != null && selectedTagIds.Length > 0)
                {
                    var selectedTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
                    existingPost.Tags.AddRange(selectedTags);
                }

                // Gán/tạo thêm các thẻ mới từ Textbox
                if (!string.IsNullOrWhiteSpace(newTags))
                {
                    var tagNames = newTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(t => t.Trim())
                                          .Where(t => !string.IsNullOrEmpty(t))
                                          .Distinct(StringComparer.OrdinalIgnoreCase);

                    foreach (var tagName in tagNames)
                    {
                        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                        if (existingTag != null)
                        {
                            if (!existingPost.Tags.Any(t => t.Id == existingTag.Id))
                            {
                                existingPost.Tags.Add(existingTag);
                            }
                        }
                        else
                        {
                            existingPost.Tags.Add(new Tag { Name = tagName });
                        }
                    }
                }

                _context.Update(existingPost);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Posts.Any(e => e.Id == post.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Hàm xóa dấu tiếng Việt
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