using Blogmanager_phamvanbinhminh.Models;

namespace Blogmanager_phamvanbinhminh.ViewModels
{
    public class PostListViewModel
    {
        public List<Post> Posts { get; set; } = new List<Post>();
        public string? Search { get; set; }

        public string? Sort { get; set; }
        public int? CurrentPage { get; set; }
        public int? TotalPages { get; set; }
    }
}