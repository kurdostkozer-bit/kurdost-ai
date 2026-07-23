# Kurdost AI - Unity Frontend

Beautiful AI chat interface for Unity Editor.

## Features

✅ **Chat Interface**
- Real-time conversation with AI
- Message history
- Beautiful dark theme

✅ **Multi-Provider Support**
- Groq (fast & free)
- Gemini (Google)

✅ **Configuration**
- API key management
- Model selection
- Temperature & token settings

## Installation

1. **Assets → Import Package → Custom Package**
2. Select `com.kurdost.ai.unitypackage`
3. Click **Import**

Then:
- **Window → Kurdost AI → Main**
- Enter your Groq API key from https://console.groq.com/keys
- Start chatting!

## Backend Setup

The frontend needs the backend server running:

```bash
cd backend
npm install
npm run build
npm start
```

Server: `http://localhost:3000`

## How It Works

1. User enters message in chat window
2. Frontend sends to backend: `POST /api/v1/chat`
3. Backend calls AI provider (Groq/Gemini)
4. Response displayed in chat

## Files

- `KurdostAIChatService.cs` - Backend communication
- `KurdostAIMainWindow.cs` - UI & chat interface
- `KurdostAI.Editor.asmdef` - Assembly definition

## Support

https://github.com/kurdostkozer-bit/kurdost-ai
