using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using MyBlazorApp.Constants;

namespace MyBlazorApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslateController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslateController> _logger;

    public TranslateController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TranslateController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
    {
        try
        {
            var apiKey = _configuration[ApiConstants.ConfigKeys.HuggingFaceApiKey];

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest(new { error = ApiConstants.ErrorMessages.ApiKeyNotConfigured });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Use different translation models based on direction
            string model = request.Direction == "en-to-zh"
                ? _configuration[ApiConstants.ConfigKeys.HuggingFaceTranslationModelEnZh] ?? ApiConstants.DefaultValues.DefaultTranslationModelEnZh
                : _configuration[ApiConstants.ConfigKeys.HuggingFaceTranslationModelZhEn] ?? ApiConstants.DefaultValues.DefaultTranslationModelZhEn;

            var payload = new
            {
                inputs = request.Text
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"https://api-inference.huggingface.co/models/{model}",
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

            // Parse translation response
            string translatedText = "No translation";

            // The API returns an array with translation objects
            if (result.ValueKind == JsonValueKind.Array && result.GetArrayLength() > 0)
            {
                var firstResult = result[0];
                if (firstResult.TryGetProperty("translation_text", out var translation))
                {
                    translatedText = translation.GetString() ?? "No translation";
                }
            }

            // Generate pinyin if the result contains Chinese characters
            string pinyin = "";
            if (request.Direction == "en-to-zh" && !string.IsNullOrEmpty(translatedText))
            {
                pinyin = await GeneratePinyin(client, apiKey, translatedText);
            }

            return Ok(new TranslateResponse
            {
                Translation = translatedText,
                Pinyin = pinyin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing translation request");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private async Task<string> GeneratePinyin(HttpClient client, string apiKey, string chineseText)
    {
        try
        {
            // Use an LLM to generate pinyin
            var model = _configuration[ApiConstants.ConfigKeys.HuggingFaceChatModel] ?? ApiConstants.DefaultValues.DefaultChatModel;

            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = $"Convert the following Chinese text to pinyin with tone marks. Only output the pinyin, nothing else: {chineseText}" }
                },
                max_tokens = 500,
                temperature = 0.3
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "https://router.huggingface.co/v1/chat/completions",
                content);

            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (result.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var messageContent))
                {
                    return messageContent.GetString()?.Trim() ?? "";
                }
            }

            return "";
        }
        catch
        {
            return "";
        }
    }
}

public class TranslateRequest
{
    public string Text { get; set; } = string.Empty;
    public string Direction { get; set; } = "en-to-zh"; // en-to-zh or zh-to-en
}

public class TranslateResponse
{
    public string Translation { get; set; } = string.Empty;
    public string Pinyin { get; set; } = string.Empty;
}