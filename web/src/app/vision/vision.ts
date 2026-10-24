import { Component, inject, signal } from '@angular/core';
import { ApiService, VisionAnalysis } from '../api.service';
import { AnswerBox } from '../shared/answer-box/answer-box';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Upload a picture: the chat model describes it, the Vision service reads its text. */
@Component({
  selector: 'app-vision',
  imports: [FeatureCard, AnswerBox, ErrorMessage],
  templateUrl: './vision.html',
})
export class Vision {
  private api = inject(ApiService);

  preview = signal('');
  result = signal<VisionAnalysis | null>(null);
  loading = signal(false);
  error = signal('');

  async analyse(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.preview.set(URL.createObjectURL(file));
    this.loading.set(true);
    this.error.set('');
    this.result.set(null);

    try {
      this.result.set(await this.api.vision(file));
    } catch {
      this.error.set('Das Bild konnte nicht analysiert werden (max. 4 MB, JPEG/PNG/WebP/GIF).');
    } finally {
      this.loading.set(false);
    }
  }
}
