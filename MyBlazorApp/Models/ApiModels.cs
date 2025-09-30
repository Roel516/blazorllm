using System.ComponentModel.DataAnnotations;

namespace MyBlazorApp.Models;

public class ChatRequest
{
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
}

public class ImageRequest
{
    [Required(ErrorMessage = "Prompt is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Prompt must be between 1 and 500 characters")]
    public string Prompt { get; set; } = string.Empty;
}

public class ImageResponse
{
    public string ImageData { get; set; } = string.Empty;
}

public class TranslateRequest
{
    [Required(ErrorMessage = "Text is required")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Text must be between 1 and 5000 characters")]
    public string Text { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(en-to-zh|zh-to-en)$", ErrorMessage = "Invalid translation direction")]
    public string Direction { get; set; } = "en-to-zh";
}

public class TranslateResponse
{
    public string Translation { get; set; } = string.Empty;
    public string Pinyin { get; set; } = string.Empty;
}

public class GeneratedImage
{
    public string ImageData { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
