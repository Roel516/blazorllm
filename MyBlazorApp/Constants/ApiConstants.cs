namespace MyBlazorApp.Constants;

public static class ApiConstants
{
    public static class ConfigKeys
    {
        public const string HuggingFaceApiKey = "HuggingFace:ApiKey";
        public const string HuggingFaceModel = "HuggingFace:Model";
        public const string HuggingFaceImageModel = "HuggingFace:ImageModel";
        public const string ApiBaseUrl = "ApiSettings:BaseUrl";
    }

    public static class Endpoints
    {
        public const string ChatCompletions = "https://router.huggingface.co/v1/chat/completions";
        public const string ImageInferenceBase = "https://api-inference.huggingface.co/models/";
    }

    public static class ErrorMessages
    {
        public const string ApiKeyNotConfigured = "HuggingFace API key not configured";
        public const string InternalServerError = "An error occurred processing your request. Please try again.";
        public const string MessageRequired = "Message cannot be empty";
        public const string PromptRequired = "Prompt cannot be empty";
        public const string TextRequired = "Text cannot be empty";
    }

    public static class Limits
    {
        public const int MaxChatMessageLength = 2000;
        public const int MaxImagePromptLength = 500;
        public const int MaxTranslationLength = 5000;
        public const int MaxImageHistoryCount = 6;
    }

    public static class DefaultValues
    {
        public const string DefaultModel = "google/gemma-2-2b-it";
        public const string DefaultImageModel = "black-forest-labs/FLUX.1-schnell";
        public const int DefaultChatMaxTokens = 500;
        public const double DefaultChatTemperature = 0.7;
        public const double DefaultPinyinTemperature = 0.3;
    }
}
