using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MyBlazorApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageController> _logger;

    public ImageController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ImageController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateImage([FromBody] ImageRequest request)
    {
        try
        {
            var apiKey = _configuration["HuggingFace:ApiKey"];
            var model = _configuration["HuggingFace:ImageModel"] ?? "black-forest-labs/FLUX.1-schnell";

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest(new { error = "HuggingFace API key not configured" });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.Timeout = TimeSpan.FromMinutes(2); // Image generation can take time

            var payload = new
            {
                inputs = request.Prompt
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"https://api-inference.huggingface.co/models/{model}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("HuggingFace API error: {StatusCode} - {Response}",
                    response.StatusCode, errorContent);
                return StatusCode((int)response.StatusCode,
                    new { error = "Error from HuggingFace API", details = errorContent });
            }

            // Get the image bytes
            var imageBytes = await response.Content.ReadAsByteArrayAsync();

            // Convert to base64 for easy transfer to frontend
            var base64Image = Convert.ToBase64String(imageBytes);

            return Ok(new ImageResponse
            {
                ImageData = $"data:image/jpeg;base64,{base64Image}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image generation request");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

public class ImageRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public class ImageResponse
{
    public string ImageData { get; set; } = string.Empty;
}