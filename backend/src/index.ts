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
    const { provider, messages, model, temperature, max_tokens, context } = req.body;
    const apiKey = req.headers['x-api-key'] as string;

    console.log('📥 Request received:', { provider, messagesCount: messages?.length, model, temperature, max_tokens, hasApiKey: !!apiKey, hasContext: !!context });

    if (!provider) {
      return res.status(400).json({ error: 'Provider is required' });
    }

    if (!messages || !Array.isArray(messages)) {
      return res.status(400).json({ error: 'Messages array is required' });
    }

    // Build messages array with context as system message
    const enhancedMessages: Message[] = [];

    // Add context as system message if provided
    if (context) {
      const contextString = typeof context === 'string' ? context : JSON.stringify(context, null, 2);
      const contextSize = Buffer.byteLength(contextString, 'utf8');
      console.log(`📊 Context size: ${contextSize} bytes (${(contextSize / 1024).toFixed(2)} KB)`);

      // Truncate context if too large (limit to 50KB to avoid token limits)
      const maxContextSize = 50 * 1024; // 50KB
      let finalContextString = contextString;
      if (contextSize > maxContextSize) {
        console.warn(`⚠️ Context too large (${contextSize} bytes), truncating to ${maxContextSize} bytes`);
        finalContextString = contextString.substring(0, maxContextSize) + '\n\n[Context truncated due to size limit]';
      }

      enhancedMessages.push({
        role: 'system',
        content: `You are a Unity AI assistant. Here is the current Unity project context:\n\n${finalContextString}\n\nIMPORTANT RULES:\n1. ONLY rely on files and data explicitly shown in the ScriptAnalysis and ProjectStructure sections.\n2. DO NOT hallucinate or assume the existence of files that are not listed.\n3. DO NOT invent methods, classes, or properties that are not explicitly mentioned.\n4. When analyzing scripts, ONLY use the metadata provided (Class, Methods, SerializedFields, BaseClass, Namespace).\n5. Do NOT mix methods between different scripts - each method belongs to the specific script it's listed under.\n6. If you're unsure about something, state that you don't have enough information rather than guessing.\n\nUse this context to provide accurate and relevant answers about the project.`
      });
    }

    // Add user messages
    enhancedMessages.push(...messages);

    // Use API key from header if provided, otherwise use environment variable
    let effectiveApiKey = apiKey;
    if (!effectiveApiKey && provider === 'groq') {
      effectiveApiKey = process.env.GROQ_API_KEY || '';
    } else if (!effectiveApiKey && provider === 'gemini') {
      effectiveApiKey = process.env.GEMINI_API_KEY || '';
    }

    if (!effectiveApiKey) {
      console.error('❌ No API key available');
      return res.status(400).json({ error: 'API key is required (either in header or environment variable)' });
    }

    console.log(`🔑 Using API key from: ${apiKey ? 'header' : 'environment'}`);

    // Temporarily override the provider's API key with the one from request
    const tempToolkit = new AIToolkit();
    if (provider === 'groq') {
      console.log(`📝 Registering Groq provider with model: ${model}, temperature: ${temperature}, maxTokens: ${max_tokens}`);
      tempToolkit.registerProvider('groq', new GroqProvider({
        apiKey: effectiveApiKey,
        model: model || 'llama-3.1-8b-instant',
        temperature: temperature || 0.7,
        maxTokens: max_tokens || 1000
      }));
    } else if (provider === 'gemini') {
      tempToolkit.registerProvider('gemini', new GeminiProvider({ apiKey: effectiveApiKey }));
    } else {
      console.error(`❌ Unknown provider: ${provider}`);
      return res.status(400).json({ error: `Unknown provider: ${provider}` });
    }

    console.log(`📝 Chat request: provider=${provider}, messages=${enhancedMessages.length}, model=${model}, temperature=${temperature}, max_tokens=${max_tokens}`);

    const response = await tempToolkit.sendMessage(provider, enhancedMessages);

    res.json({
      success: true,
      message: response,
      provider,
      timestamp: new Date().toISOString(),
    });
  } catch (error: any) {
    console.error('❌ Chat error:', error.message);
    console.error('❌ Error stack:', error.stack);
    console.error('❌ Error details:', error);
    
    // Log additional error details if available
    if (error.response) {
      console.error('❌ Error response:', error.response.data);
    }
    if (error.code) {
      console.error('❌ Error code:', error.code);
    }
    
    res.status(500).json({
      success: false,
      error: error.message,
      details: error.response?.data || error.stack,
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
