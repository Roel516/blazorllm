# AI Studio

A modern Blazor web application featuring AI-powered tools using HuggingFace models.

## Features

- **💬 AI Chat** - Interactive chat interface powered by Google Gemma 2B model
- **🖼️ Image Generator** - Generate images from text descriptions using Stable Diffusion
- **🌐 Translator** - English ⇄ Chinese translation with pinyin pronunciation support

## Tech Stack

- **Framework**: ASP.NET Core Blazor (.NET 9)
- **UI**: Modern, responsive design with gradient accents
- **AI Models**: HuggingFace API integration
  - Chat: `google/gemma-2-2b-it`
  - Translation: `Helsinki-NLP/opus-mt-en-zh` / `Helsinki-NLP/opus-mt-zh-en`
  - Image Generation: `stabilityai/stable-diffusion-2-1`

## Getting Started

### Prerequisites

- .NET 9 SDK
- HuggingFace API key

### Installation

1. Clone the repository
```bash
git clone https://github.com/Roel516/blazorllm.git
cd blazorllm
```

2. Configure your HuggingFace API key in `MyBlazorApp/appsettings.json`:
```json
{
  "HuggingFace": {
    "ApiKey": "your-api-key-here",
    "Model": "google/gemma-2-2b-it"
  }
}
```

3. Run the application
```bash
cd MyBlazorApp
dotnet run
```

4. Open your browser to `http://localhost:5000`

## Project Structure

```
MyBlazorApp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor    # Main app layout with sidebar
│   │   └── NavMenu.razor        # Navigation menu
│   └── Pages/
│       ├── Chat.razor           # AI chat page
│       ├── ImageGen.razor       # Image generation page
│       └── Translate.razor      # Translation page
├── Controllers/
│   ├── ChatController.cs        # Chat API endpoint
│   ├── ImageController.cs       # Image generation API endpoint
│   └── TranslateController.cs   # Translation API endpoint
└── wwwroot/
    └── app.css                  # Global styles
```

## Features Detail

### Chat
- Real-time AI responses
- Clean, modern chat interface
- Message history display

### Image Generator
- Text-to-image generation
- Base64 encoded image display
- Error handling and loading states

### Translator
- Bidirectional English-Chinese translation
- Automatic pinyin generation for Chinese text
- Side-by-side input/output layout

## Design

Inspired by modern corporate websites with:
- Clean, minimalist aesthetic
- Gradient accents (blue → teal → green → orange)
- Professional typography
- Smooth transitions and hover effects
- Responsive layout

## API Endpoints

- `POST /api/chat/send` - Send chat messages
- `POST /api/image/generate` - Generate images from text
- `POST /api/translate/translate` - Translate text

## License

MIT

## Contributing

Pull requests welcome! Please ensure your code follows the existing style conventions.
