import axios from 'axios';

export interface GroqConfig {
  apiKey: string;
  model?: string;
  temperature?: number;
  maxTokens?: number;
}

export class GroqProvider {
  private apiKey: string;
  private model: string;
  private temperature: number;
  private maxTokens: number;
  private baseUrl = 'https://api.groq.com/openai/v1';

  constructor(config: GroqConfig) {
    this.apiKey = config.apiKey;
    this.model = config.model || 'llama-3.1-8b-instant';
    this.temperature = config.temperature || 0.7;
    this.maxTokens = config.maxTokens || 2048;
  }

  async send(messages: Array<{ role: string; content: string }>): Promise<string> {
    const maxRetries = 3;
    let retryCount = 0;

    while (retryCount < maxRetries) {
      try {
        const response = await axios.post(
          `${this.baseUrl}/chat/completions`,
          {
            model: this.model,
            messages,
            temperature: this.temperature,
            max_tokens: this.maxTokens,
          },
          {
            headers: {
              Authorization: `Bearer ${this.apiKey}`,
              'Content-Type': 'application/json',
            },
            timeout: 30000,
          }
        );

        return response.data.choices[0].message.content;
      } catch (error: any) {
        const isRateLimit = error.response?.status === 429;
        
        if (isRateLimit && retryCount < maxRetries - 1) {
          const retryAfter = error.response?.data?.error?.message?.match(/try again in ([\d.]+)s/)?.[1];
          const delay = retryAfter ? parseFloat(retryAfter) * 1000 : Math.pow(2, retryCount) * 1000;
          
          console.warn(`⚠️ Rate limit hit, retrying in ${delay}ms (attempt ${retryCount + 1}/${maxRetries})`);
          await new Promise(resolve => setTimeout(resolve, delay));
          retryCount++;
        } else {
          console.error('Groq API Error:', error.response?.data || error.message);
          throw new Error(`Groq API Error: ${error.message}`);
        }
      }
    }

    throw new Error('Groq API Error: Max retries exceeded');
  }
}
