using System.Security.Claims; // 👈 Dùng để lấy ID của User đang đăng nhập
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Models;
using Blogmanager_phamvanbinhminh.ViewModels;
using System.Text;
using System.Globalization;

namespace Blogmanager_phamvanbinhminh.Controllers
{
    [Authorize] // 🔥 Yêu cầu ĐĂNG NHẬP cho toàn bộ Controller
    public class PostsController : Controller
    {
        private readonly ApplicationDBContext _context;

        public PostsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // 1. INDEX: Khách vãng lai xem danh sách
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, string? sort, int page = 1)
        {
            int pageSize = 5;

            var postsQuery = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .ToListAsync();

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

            var query = postsQuery.AsQueryable();
            query = sort switch
            {
                "title" => query.OrderBy(p => p.Title),
                "oldest" => query.OrderBy(p => p.PublishedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

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

        // 2. DETAILS: Khách vãng lai xem chi tiết
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // 3. CREATE (Yêu cầu Đăng nhập)
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
        post.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (selectedTagIds != null && selectedTagIds.Length > 0)
            {
                var existingTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
                post.Tags.AddRange(existingTags);
            }

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

        // 4. EDIT (Yêu cầu Đăng nhập)
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!User.IsInRole("Admin") && post.OwnerId != currentUserId)
    {
        return Forbid(); // Trả về lỗi 403 Chặn truy cập
    }
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
            
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && existingPost.OwnerId != currentUserId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.SelectedTagIds = selectedTagIds?.ToList() ?? new List<int>();
                ViewBag.NewTags = newTags;
                return View(post);
            }
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.Author = post.Author;
                existingPost.PublishedAt = post.PublishedAt;
                existingPost.IsPublished = post.IsPublished;
                existingPost.CategoryId = post.CategoryId;

                existingPost.Tags.Clear();

                if (selectedTagIds != null && selectedTagIds.Length > 0)
                {
                    var selectedTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
                    existingPost.Tags.AddRange(selectedTags);
                }

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

        // 5. DELETE (Yêu cầu Đăng nhập)
        [Authorize] // 🔥 Chỉ Admin mới được xóa bài viết
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null) return NotFound();
        
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && post.OwnerId != currentUserId)
        {
            return Forbid();
        }  

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize] // 🔥 Chỉ Admin mới được xóa bài viết
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Admin") && post.OwnerId != currentUserId)
                {
                    return Forbid();
                }
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

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