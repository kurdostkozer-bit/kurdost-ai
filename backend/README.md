# Backend Server

Express.js server for AI chat.

## Setup

```bash
npm install
npm run build
npm start
```

## Environment

Create `.env`:
```env
GROQ_API_KEY=your_key
GEMINI_API_KEY=your_key
PORT=3000
```

## API Endpoints

- `GET /health` - Health check
- `GET /providers` - List providers
- `POST /api/v1/chat` - Chat endpoint

## Development

```bash
npm run dev
```

Watches and rebuilds on changes.
