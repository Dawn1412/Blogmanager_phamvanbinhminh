using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Models;

namespace Blogmanager_phamvanbinhminh.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>(); 
    }
}