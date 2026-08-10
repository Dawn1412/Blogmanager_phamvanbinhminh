using System.ComponentModel.DataAnnotations;

namespace Blogmanager_phamvanbinhminh.Models;

public class Post
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung là bắt buộc")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    // THÊM THUỘC TÍNH NÀY VÀO POST.CS
    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày đăng")]
    [DataType(DataType.Date)]
    public DateTime PublishedAt { get; set; } = DateTime.Now;

    public bool IsPublished { get; set; } = true;

    [Display(Name = "Tác giả")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Lượt xem")]
    public int ViewCount { get; set; }

    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }           // Khóa ngoại (Foreign Key)
    public Category? Category { get; set; }       // Navigation property

    public List<Tag> Tags { get; set; } = new();   // Navigation property

    public string MoTaNgan() => $"{Title} ({PublishedAt:dd/MM/yyyy})";
}