import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../api.service';
import { AnswerBox } from '../shared/answer-box/answer-box';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Ask the chat model a question. */
@Component({
  selector: 'app-chat',
  imports: [FormsModule, FeatureCard, AnswerBox, ErrorMessage],
  templateUrl: './chat.html',
})
export class Chat {
  private api = inject(ApiService);

  question = signal('');
  answer = signal('');
  loading = signal(false);
  error = signal('');

  async ask(): Promise<void> {
    if (!this.question().trim()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.answer.set('');

    try {
      const result = await this.api.chat(this.question());
      this.answer.set(result.answer);
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.loading.set(false);
    }
  }
}
