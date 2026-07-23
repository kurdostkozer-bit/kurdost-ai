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
      console.error('Groq API Error:', error.response?.data || error.message);
      throw new Error(`Groq API Error: ${error.message}`);
    }
  }
}
