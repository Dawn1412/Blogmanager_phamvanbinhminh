using Blogmanager_phamvanbinhminh.Data;
using Blogmanager_phamvanbinhminh.Dtos;
using Blogmanager_phamvanbinhminh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogmanager_phamvanbinhminh.Controllers.Api
{
    [ApiController]
    [Route("api/posts")]
    public class PostsApiController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public PostsApiController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET /api/posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
        {
            var posts = await _context.Posts
                .Select(p => new PostDto { Id = p.Id, Title = p.Title, Author = p.Author, PublishedAt = p.PublishedAt, IsPublished = p.IsPublished })
                .ToListAsync();
            return Ok(posts); // 200
        }

        // GET /api/posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            var p = await _context.Posts.FindAsync(id);
            if (p == null) return NotFound(); // 404
            return Ok(new PostDto { Id = p.Id, Title = p.Title, Author = p.Author, PublishedAt = p.PublishedAt, IsPublished = p.IsPublished });
        }

        // POST /api/posts
        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost(PostCreateDto dto)
        {
            var post = new Post { Title = dto.Title, Content = dto.Content, Author = dto.Author };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var result = new PostDto { Id = post.Id, Title = post.Title, Author = post.Author, PublishedAt = post.PublishedAt, IsPublished = post.IsPublished };
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, result); // 201
        }

        // PUT /api/posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, PostCreateDto dto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound(); // 404

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.Author = dto.Author;

            await _context.SaveChangesAsync();
            return NoContent(); // 204
        }

        // DELETE /api/posts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 👈 Chỉ tài khoản có quyền Admin mới được thực thi
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound(); // 404

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent(); // 204
        }
    }
}