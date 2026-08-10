using Blogmanager_phamvanbinhminh.Models;

namespace Blogmanager_phamvanbinhminh.ViewModels
{
    public class CategoryListViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? Search { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}