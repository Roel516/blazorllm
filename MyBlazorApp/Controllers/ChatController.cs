using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using MyBlazorApp.Models;
using MyBlazorApp.Constants;

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
        // Validation is handled by data annotations, but add extra check
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var apiKey = _configuration[ApiConstants.ConfigKeys.HuggingFaceApiKey];
            var model = _configuration[ApiConstants.ConfigKeys.HuggingFaceChatModel]
                ?? ApiConstants.DefaultValues.DefaultChatModel;

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("HuggingFace API key not configured");
                return BadRequest(new { error = ApiConstants.ErrorMessages.ApiKeyNotConfigured });
            }

            var client = _httpClientFactory.CreateClient("HuggingFaceClient");

            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = request.Message }
                },
                max_tokens = ApiConstants.DefaultValues.DefaultChatMaxTokens,
                temperature = ApiConstants.DefaultValues.DefaultChatTemperature
            };

            using (var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"))
            {
                var response = await client.PostAsync(
                    ApiConstants.Endpoints.ChatCompletions,
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HuggingFace API error: {StatusCode} - Response length: {Length}",
                        response.StatusCode, responseContent.Length);
                    return StatusCode((int)response.StatusCode,
                        new { error = "Error from HuggingFace API" });
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
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error processing chat request");
            return StatusCode(503, new { error = ApiConstants.ErrorMessages.InternalServerError });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout processing chat request");
            return StatusCode(504, new { error = "Request timed out. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request for message length: {Length}",
                request.Message?.Length ?? 0);
            return StatusCode(500, new { error = ApiConstants.ErrorMessages.InternalServerError });
        }
    }
}
