import { GroqProvider, GeminiProvider, Provider, Message, ProviderInstance } from '../providers';

export class AIToolkit {
  private providers: Map<string, ProviderInstance> = new Map();

  registerProvider(name: Provider, provider: ProviderInstance): void {
    this.providers.set(name, provider);
    console.log(`✅ Provider registered: ${name}`);
  }

  getProvider(name: string): ProviderInstance | undefined {
    return this.providers.get(name);
  }

  listProviders(): string[] {
    return Array.from(this.providers.keys());
  }

  async sendMessage(provider: string, messages: Message[]): Promise<string> {
    const providerInstance = this.getProvider(provider);
    if (!providerInstance) {
      throw new Error(`Provider ${provider} not found`);
    }
    return await providerInstance.send(messages);
  }
}
