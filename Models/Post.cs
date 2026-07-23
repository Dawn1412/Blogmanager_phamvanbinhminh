namespace Blogmanager_phamvanbinhminh.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.Now;
    public bool IsPublished { get; set; }

    public string MoTaNgan() => $"{Title} ({PublishedAt:dd/MM/yyyy})";
}