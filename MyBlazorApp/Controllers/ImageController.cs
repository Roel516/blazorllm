using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using MyBlazorApp.Models;
using MyBlazorApp.Constants;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var apiKey = _configuration[ApiConstants.ConfigKeys.HuggingFaceApiKey];
            var imageModel = _configuration[ApiConstants.ConfigKeys.HuggingFaceImageModel]
                ?? ApiConstants.DefaultValues.DefaultImageModel;

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("HuggingFace API key not configured");
                return BadRequest(new { error = ApiConstants.ErrorMessages.ApiKeyNotConfigured });
            }

            var client = _httpClientFactory.CreateClient("ImageGenerationClient");

            var payload = new { inputs = request.Prompt };

            using (var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"))
            {
                var modelUrl = $"{ApiConstants.Endpoints.ImageInferenceBase}{imageModel}";
                var response = await client.PostAsync(modelUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("HuggingFace Image API error: {StatusCode} - Response length: {Length}",
                        response.StatusCode, errorContent.Length);

                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        return StatusCode(503, new { error = "Model is loading. Please try again in a moment." });
                    }

                    return StatusCode((int)response.StatusCode,
                        new { error = "Error generating image" });
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var base64Image = Convert.ToBase64String(imageBytes);

                return Ok(new ImageResponse
                {
                    ImageData = $"data:image/png;base64,{base64Image}"
                });
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error processing image generation request");
            return StatusCode(503, new { error = ApiConstants.ErrorMessages.InternalServerError });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout processing image generation request");
            return StatusCode(504, new { error = "Image generation timed out. Please try again with a simpler prompt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image generation for prompt length: {Length}",
                request.Prompt?.Length ?? 0);
            return StatusCode(500, new { error = ApiConstants.ErrorMessages.InternalServerError });
        }
    }
}
