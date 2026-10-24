import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../api.service';
import { AnswerBox } from '../shared/answer-box/answer-box';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Upload a text file, then ask questions that are answered only from that file. */
@Component({
  selector: 'app-rag',
  imports: [FormsModule, FeatureCard, AnswerBox, ErrorMessage],
  templateUrl: './rag.html',
})
export class Rag {
  private api = inject(ApiService);

  documentName = signal('');
  chunkCount = signal(0);
  uploading = signal(false);

  question = signal('');
  answer = signal('');
  sources = signal<string[]>([]);
  asking = signal(false);
  error = signal('');

  async upload(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.uploading.set(true);
    this.error.set('');
    this.answer.set('');
    this.sources.set([]);

    try {
      const status = await this.api.ragUpload(file);
      this.documentName.set(status.documentName);
      this.chunkCount.set(status.chunkCount);
    } catch {
      this.error.set('Die Datei konnte nicht gelesen werden (max. 200 KB, nur Text).');
    } finally {
      this.uploading.set(false);
    }
  }

  async ask(): Promise<void> {
    if (!this.question().trim()) {
      return;
    }

    this.asking.set(true);
    this.error.set('');
    this.answer.set('');
    this.sources.set([]);

    try {
      const result = await this.api.ragAsk(this.question());
      this.answer.set(result.answer);
      this.sources.set(result.sources);
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.asking.set(false);
    }
  }
}
