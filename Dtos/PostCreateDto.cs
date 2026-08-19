using System.ComponentModel.DataAnnotations;

namespace Blogmanager_phamvanbinhminh.Dtos
{
    public class PostCreateDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;
    }
}