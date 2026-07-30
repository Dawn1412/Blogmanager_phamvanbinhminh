using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Models;

namespace Blogmanager_phamvanbinhminh.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }
        
        // Thêm DbSet cho Category
        public DbSet<Category> Categories { get; set; }
    }
}