using Microsoft.EntityFrameworkCore;
using Blogmanager_phamvanbinhminh.Models;   

namespace Blogmanager_phamvanbinhminh.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Post> Posts { get; set; } = new();
    }
}