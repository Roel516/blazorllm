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
- Docker (optional, for containerized deployment)
- Kubernetes cluster (optional, for Kubernetes deployment)
- Helm 3+ (optional, for Helm deployment)

### Installation

#### Option 1: Run with .NET

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

#### Option 2: Run with Docker

1. Clone the repository
```bash
git clone https://github.com/Roel516/blazorllm.git
cd blazorllm
```

2. Build the Docker image
```bash
docker build -t myblazorapp:latest .
```

3. Run the container
```bash
docker run -p 8080:8080 \
  -e HuggingFace__ApiKey="your-api-key-here" \
  -e ApiSettings__BaseUrl="http://localhost:8080" \
  myblazorapp:latest
```

4. Open your browser to `http://localhost:8080`

#### Option 3: Deploy to Kubernetes

1. Clone the repository
```bash
git clone https://github.com/Roel516/blazorllm.git
cd blazorllm
```

2. Build and push the Docker image to your registry
```bash
docker build -t your-registry/myblazorapp:latest .
docker push your-registry/myblazorapp:latest
```

3. Update the image in `k8s-deployment.yaml` to point to your registry

4. Update the HuggingFace API key in `k8s-configmap.yaml`

5. Deploy to Kubernetes
```bash
kubectl apply -f k8s-configmap.yaml
kubectl apply -f k8s-deployment.yaml
kubectl apply -f k8s-service.yaml
```

6. Get the external IP of the service
```bash
kubectl get service myblazorapp
```

7. Access the application at `http://<EXTERNAL-IP>`

#### Option 4: Deploy with Helm

1. Clone the repository
```bash
git clone https://github.com/Roel516/blazorllm.git
cd blazorllm
```

2. Build and push the Docker image to your registry
```bash
docker build -t your-registry/myblazorapp:latest .
docker push your-registry/myblazorapp:latest
```

3. Install the Helm chart
```bash
# Install with default values
helm install myblazorapp ./helm/myblazorapp

# Or install with custom values
helm install myblazorapp ./helm/myblazorapp \
  --set image.repository=your-registry/myblazorapp \
  --set image.tag=latest \
  --set huggingface.apiKey="your-api-key-here"
```

4. Check the deployment status
```bash
helm status myblazorapp
kubectl get pods
```

5. Access the application (follow the instructions from `helm status myblazorapp`)

#### Helm Configuration

Key configuration options in `helm/myblazorapp/values.yaml`:

- **Replicas**: `replicaCount: 3`
- **Image**: `image.repository` and `image.tag`
- **Service Type**: `service.type: LoadBalancer` (can be ClusterIP, NodePort, or LoadBalancer)
- **Resources**: CPU and memory limits/requests
- **HuggingFace API Key**: `huggingface.apiKey` or use existing secret with `huggingface.existingSecret`
- **Autoscaling**: Enable with `autoscaling.enabled: true`
- **Ingress**: Enable with `ingress.enabled: true`

Example custom deployment:
```bash
helm install myblazorapp ./helm/myblazorapp \
  --set replicaCount=5 \
  --set image.repository=your-registry/myblazorapp \
  --set image.tag=v1.0.0 \
  --set service.type=ClusterIP \
  --set ingress.enabled=true \
  --set ingress.hosts[0].host=myblazorapp.example.com \
  --set huggingface.apiKey="your-api-key-here"
```

To upgrade an existing release:
```bash
helm upgrade myblazorapp ./helm/myblazorapp
```

To uninstall:
```bash
helm uninstall myblazorapp
```

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
