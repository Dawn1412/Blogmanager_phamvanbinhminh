using System.ComponentModel.DataAnnotations;

namespace Blogmanager_phamvanbinhminh.ViewModels;

public class PostCreateViewModel
{
    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(200, MinimumLength = 3)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung là bắt buộc")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tác giả là bắt buộc")]
    [Display(Name = "Tác giả")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Ngày đăng")]
    [DataType(DataType.Date)]
    public DateTime PublishedAt { get; set; } = DateTime.Now;

    [Display(Name = "Đã xuất bản")]
    public bool IsPublished { get; set; }
}