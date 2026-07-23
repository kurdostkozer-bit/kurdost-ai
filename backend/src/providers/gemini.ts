import axios from 'axios';

export interface GeminiConfig {
  apiKey: string;
  model?: string;
}

export class GeminiProvider {
  private apiKey: string;
  private model: string;
  private baseUrl = 'https://generativelanguage.googleapis.com/v1beta/models';

  constructor(config: GeminiConfig) {
    this.apiKey = config.apiKey;
    this.model = config.model || 'gemini-pro';
  }

  async send(messages: Array<{ role: string; content: string }>): Promise<string> {
    try {
      const response = await axios.post(
        `${this.baseUrl}/${this.model}:generateContent?key=${this.apiKey}`,
        {
          contents: messages.map((msg) => ({
            parts: [{ text: msg.content }],
          })),
        },
        {
          headers: {
            'Content-Type': 'application/json',
          },
          timeout: 30000,
        }
      );

      return response.data.candidates[0].content.parts[0].text;
    } catch (error: any) {
      console.error('Gemini API Error:', error.response?.data || error.message);
      throw new Error(`Gemini API Error: ${error.message}`);
    }
  }
}
