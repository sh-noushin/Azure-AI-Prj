import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, LanguageOption } from '../api.service';
import { AnswerBox } from '../shared/answer-box/answer-box';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Translate text. The source language is detected automatically. */
@Component({
  selector: 'app-translate',
  imports: [FormsModule, FeatureCard, AnswerBox, ErrorMessage],
  templateUrl: './translate.html',
})
export class Translate {
  private api = inject(ApiService);

  text = signal('Wie exportiere ich ein Dokument als PDF?');
  languages = signal<LanguageOption[]>([]);
  target = signal('en');

  result = signal('');
  detected = signal('');
  loading = signal(false);
  error = signal('');

  constructor() {
    this.api.translateLanguages().then((languages) => this.languages.set(languages));
  }

  async translate(): Promise<void> {
    if (!this.text().trim()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.result.set('');

    try {
      const translation = await this.api.translate(this.text(), this.target());
      this.result.set(translation.text);
      this.detected.set(translation.detectedLanguage);
    } catch {
      this.error.set('Die Übersetzung ist fehlgeschlagen.');
    } finally {
      this.loading.set(false);
    }
  }
}
