import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, VoiceOption } from '../api.service';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';

/** Two directions: text read out loud, and an audio file written down. */
@Component({
  selector: 'app-speech',
  imports: [FormsModule, FeatureCard, ErrorMessage],
  templateUrl: './speech.html',
})
export class Speech {
  private api = inject(ApiService);

  // --- text to speech ---
  text = signal('Hallo! Azure liest diesen Satz vor.');
  voices = signal<VoiceOption[]>([]);
  voice = signal('de-DE-KatjaNeural');
  audioUrl = signal('');
  speaking = signal(false);

  // --- speech to text ---
  language = signal('de-DE');
  transcript = signal('');
  transcribing = signal(false);

  error = signal('');

  constructor() {
    this.api.voices().then((voices) => this.voices.set(voices));
  }

  async speak(): Promise<void> {
    if (!this.text().trim()) {
      return;
    }

    this.speaking.set(true);
    this.error.set('');

    try {
      const audio = await this.api.speak(this.text(), this.voice());
      this.audioUrl.set(URL.createObjectURL(audio));
    } catch {
      this.error.set('Die Sprachausgabe ist fehlgeschlagen.');
    } finally {
      this.speaking.set(false);
    }
  }

  async transcribe(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.transcribing.set(true);
    this.error.set('');
    this.transcript.set('');

    try {
      const result = await this.api.transcribe(file, this.language());
      this.transcript.set(result.text || 'Es wurde nichts erkannt.');
    } catch {
      this.error.set('Die Datei konnte nicht erkannt werden (WAV, max. 10 MB).');
    } finally {
      this.transcribing.set(false);
    }
  }
}
