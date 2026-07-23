export { GroqProvider, type GroqConfig } from './groq';
export { GeminiProvider, type GeminiConfig } from './gemini';

export type Provider = 'groq' | 'gemini';

export interface Message {
  role: 'user' | 'assistant';
  content: string;
}

export interface ProviderInstance {
  send(messages: Message[]): Promise<string>;
}
