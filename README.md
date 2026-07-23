# 🤖 Kurdost AI Toolkit

**Complete AI toolkit for Unity with Express backend**

Simple structure:
- **backend/** - Node.js + Express server
- **frontend/** - Unity chat package

---

## 🚀 Quick Start

### 1️⃣ Backend Setup

```bash
cd backend
npm install
npm run build
npm start
```

Server runs on: `http://localhost:3000`

### 2️⃣ Frontend Setup (Unity)

1. In Unity: **Assets → Import Package → Custom Package**
2. Select `frontend/com.kurdost.ai.unitypackage` (if built)
3. Or copy `frontend/` files to `Assets/`

Then:
- **Window → Kurdost AI → Main**
- Enter Groq API key
- Start chatting!

---

## 📋 API

### Health Check
```
GET http://localhost:3000/health
```

### List Providers
```
GET http://localhost:3000/providers
```

### Chat
```
POST http://localhost:3000/api/v1/chat

{
  "provider": "groq",
  "messages": [
    { "role": "user", "content": "Hello" }
  ]
}
```

Response:
```json
{
  "success": true,
  "message": "Response from AI",
  "provider": "groq"
}
```

---

## 🔑 Configuration

Create `.env` file in **backend/**:

```env
GROQ_API_KEY=your_groq_key
GEMINI_API_KEY=your_gemini_key
PORT=3000
```

Get free API keys:
- Groq: https://console.groq.com/keys
- Gemini: https://makersuite.google.com/app/apikey

---

## 📁 Structure

```
kurdost-ai-final/
├── backend/
│   ├── src/
│   │   ├── index.ts (Express server)
│   │   ├── core/
│   │   │   └── toolkit.ts (AI toolkit)
│   │   └── providers/
│   │       ├── groq.ts
│   │       ├── gemini.ts
│   │       └── index.ts
│   ├── dist/ (compiled)
│   ├── package.json
│   ├── tsconfig.json
│   └── README.md
│
└── frontend/
    ├── KurdostAIChatService.cs
    ├── KurdostAIMainWindow.cs
    ├── KurdostAI.Editor.asmdef
    ├── package.json
    └── README.md
```

---

## ✅ Features

**Backend**
- ✅ Express.js server
- ✅ Groq provider (fast & free)
- ✅ Gemini provider (Google)
- ✅ REST API
- ✅ CORS enabled

**Frontend (Unity)**
- ✅ Beautiful Editor window
- ✅ Chat interface with history
- ✅ API key management
- ✅ Provider selection
- ✅ Configurable settings

---

## 🧪 Testing

### Local Testing

1. Start backend:
   ```bash
   cd backend
   npm run dev
   ```

2. In Unity:
   - Open chat window: **Window → Kurdost AI → Main**
   - Enter API key
   - Send message

3. Watch backend console for logs

---

## 🌐 Deployment

### Deploy Backend to Render

1. Push to GitHub
2. Create new Web Service on Render
3. Connect GitHub repo
4. Set build command: `cd backend && npm install && npm run build`
5. Set start command: `cd backend && npm start`
6. Add environment variables: `GROQ_API_KEY`, `GEMINI_API_KEY`

### Use in Unity

1. Change `_chatUrl` in `KurdostAIChatService.cs` to your Render URL
2. Rebuild and test

---

## 📝 License

MIT

---

## 🔗 Links

- GitHub: https://github.com/kurdostkozer-bit/kurdost-ai
- Groq: https://groq.com
- Gemini: https://gemini.google.com

---

**Built from best parts of 4 projects | Simplified & Production-Ready**
