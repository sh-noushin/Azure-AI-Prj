import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../api.service';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Compare two texts by meaning, using the embedding model. */
@Component({
  selector: 'app-embeddings',
  imports: [FormsModule, FeatureCard, ErrorMessage],
  templateUrl: './embeddings.html',
})
export class Embeddings {
  private api = inject(ApiService);

  first = signal('Wie exportiere ich ein Dokument?');
  second = signal('Wo kann ich eine Datei speichern?');

  similarity = signal<number | null>(null);
  dimensions = signal(0);
  loading = signal(false);
  error = signal('');

  /** 0 to 100, for the bar in the template. */
  percent(): number {
    return Math.round((this.similarity() ?? 0) * 100);
  }

  /** A plain-language reading of the score. */
  verdict(): string {
    const value = this.similarity() ?? 0;
    if (value > 0.8) return 'Fast dasselbe';
    if (value > 0.6) return 'Ähnlich';
    if (value > 0.4) return 'Etwas verwandt';
    return 'Unterschiedlich';
  }

  async compare(): Promise<void> {
    if (!this.first().trim() || !this.second().trim()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.similarity.set(null);

    try {
      const result = await this.api.similarity(this.first(), this.second());
      this.similarity.set(result.similarity);
      this.dimensions.set(result.dimensions);
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.loading.set(false);
    }
  }
}
