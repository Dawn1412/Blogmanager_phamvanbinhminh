namespace Blogmanager_phamvanbinhminh.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }
    }
}