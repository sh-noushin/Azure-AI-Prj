import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, TextAnalysis } from '../api.service';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Analyse a text: mood, language, main topics and recognised names. */
@Component({
  selector: 'app-language',
  imports: [FormsModule, FeatureCard, ErrorMessage],
  templateUrl: './language.html',
})
export class Language {
  private api = inject(ApiService);

  text = signal(
    'Der Export funktioniert seit dem Update leider nicht mehr. Sehr ärgerlich!'
  );
  result = signal<TextAnalysis | null>(null);
  loading = signal(false);
  error = signal('');

  /** German label for the mood, so the page stays in one language. */
  sentimentLabel(): string {
    switch (this.result()?.sentiment) {
      case 'positive': return 'Positiv';
      case 'negative': return 'Negativ';
      case 'mixed': return 'Gemischt';
      default: return 'Neutral';
    }
  }

  async analyse(): Promise<void> {
    if (!this.text().trim()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.result.set(null);

    try {
      this.result.set(await this.api.language(this.text()));
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.loading.set(false);
    }
  }
}
