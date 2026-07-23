import express, { Express, Request, Response } from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import { AIToolkit } from './core/toolkit';
import { GroqProvider, GeminiProvider, Message } from './providers';

dotenv.config();

const app: Express = express();
const port = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());

// Initialize toolkit
const toolkit = new AIToolkit();

// Initialize providers from environment
if (process.env.GROQ_API_KEY) {
  toolkit.registerProvider('groq', new GroqProvider({ apiKey: process.env.GROQ_API_KEY }));
}

if (process.env.GEMINI_API_KEY) {
  toolkit.registerProvider('gemini', new GeminiProvider({ apiKey: process.env.GEMINI_API_KEY }));
}

// Routes
app.get('/health', (req: Request, res: Response) => {
  res.json({
    status: 'ok',
    timestamp: new Date().toISOString(),
    version: '1.0.0',
  });
});

app.get('/providers', (req: Request, res: Response) => {
  const providers = toolkit.listProviders();
  res.json({
    providers,
    count: providers.length,
  });
});

app.post('/api/v1/chat', async (req: Request, res: Response) => {
  try {
    const { provider, messages } = req.body;

    if (!provider) {
      return res.status(400).json({ error: 'Provider is required' });
    }

    if (!messages || !Array.isArray(messages)) {
      return res.status(400).json({ error: 'Messages array is required' });
    }

    console.log(`📝 Chat request: provider=${provider}, messages=${messages.length}`);

    const response = await toolkit.sendMessage(provider, messages);

    res.json({
      success: true,
      message: response,
      provider,
      timestamp: new Date().toISOString(),
    });
  } catch (error: any) {
    console.error('❌ Chat error:', error.message);
    res.status(500).json({
      success: false,
      error: error.message,
      timestamp: new Date().toISOString(),
    });
  }
});

// Error handling
app.use((err: any, req: Request, res: Response, next: any) => {
  console.error('Server error:', err);
  res.status(500).json({
    error: 'Internal server error',
    message: err.message,
  });
});

app.listen(port, () => {
  console.log(`✅ Server running at http://localhost:${port}`);
  console.log(`📚 Available providers: ${toolkit.listProviders().join(', ') || 'none'}`);
  console.log(`🏥 Health check: http://localhost:${port}/health`);
});
