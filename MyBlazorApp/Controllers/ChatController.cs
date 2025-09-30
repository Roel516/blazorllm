using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MyBlazorApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            var apiKey = _configuration["HuggingFace:ApiKey"];
            var model = _configuration["HuggingFace:Model"] ?? "google/gemma-2-2b-it";

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest(new { error = "HuggingFace API key not configured" });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Use the new chat completions API format
            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = request.Message }
                },
                max_tokens = 500,
                temperature = 0.7
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "https://router.huggingface.co/v1/chat/completions",
                content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HuggingFace API error: {StatusCode} - {Response}",
                    response.StatusCode, responseContent);
                return StatusCode((int)response.StatusCode,
                    new { error = "Error from HuggingFace API", details = responseContent });
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Parse chat completion response
            string responseText = "No response";
            if (result.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var messageContent))
                {
                    responseText = messageContent.GetString() ?? "No response";
                }
            }

            return Ok(new ChatResponse { Response = responseText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
}