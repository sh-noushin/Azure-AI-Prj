import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ChatTurn } from '../api.service';

/** A conversation with gpt-5-mini. The whole history is sent with every question. */
@Component({
  selector: 'app-chat',
  imports: [FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat {
  private api = inject(ApiService);
  private scroller = viewChild<ElementRef<HTMLElement>>('scroller');

  readonly examples = [
    'Was ist eine digitale Signatur?',
    'Erkläre Embeddings in drei Sätzen.',
    'Was ist der Unterschied zwischen PDF und PDF/A?',
  ];

  messages = signal<ChatTurn[]>([]);
  draft = signal('');
  loading = signal(false);
  error = signal('');

  use(example: string): void {
    this.draft.set(example);
    void this.send();
  }

  clear(): void {
    this.messages.set([]);
    this.error.set('');
  }

  /** Enter sends, Shift+Enter starts a new line. */
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      void this.send();
    }
  }

  async send(): Promise<void> {
    const question = this.draft().trim();
    if (!question || this.loading()) {
      return;
    }

    const history = this.messages();
    this.messages.set([...history, { role: 'user', text: question }]);
    this.draft.set('');
    this.error.set('');
    this.loading.set(true);
    this.scrollDown();

    try {
      const reply = await this.api.chat(question, history);
      this.messages.set([...this.messages(), { role: 'assistant', text: reply.answer }]);
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.loading.set(false);
      this.scrollDown();
    }
  }

  private scrollDown(): void {
    setTimeout(() => {
      const element = this.scroller()?.nativeElement;
      element?.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
    });
  }
}
