import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../api.service';
import { ErrorMessage } from '../shared/error-message/error-message';
import { FeatureCard } from '../shared/feature-card/feature-card';
import { toWav } from '../shared/wav';

/** Speak a question, hear the answer. Speech and the chat model working together. */
@Component({
  selector: 'app-voice',
  imports: [FeatureCard, ErrorMessage],
  templateUrl: './voice.html',
})
export class Voice {
  private api = inject(ApiService);
  private recorder?: MediaRecorder;
  private parts: Blob[] = [];

  recording = signal(false);
  thinking = signal(false);
  question = signal('');
  answer = signal('');
  audioUrl = signal('');
  error = signal('');

  async start(): Promise<void> {
    this.error.set('');

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

      this.parts = [];
      this.recorder = new MediaRecorder(stream);
      this.recorder.ondataavailable = (event) => this.parts.push(event.data);
      this.recorder.onstop = () => {
        stream.getTracks().forEach((track) => track.stop());
        void this.send();
      };

      this.recorder.start();
      this.recording.set(true);
    } catch {
      this.error.set('Kein Zugriff auf das Mikrofon. Bitte im Browser erlauben.');
    }
  }

  stop(): void {
    this.recorder?.stop();
    this.recording.set(false);
  }

  private async send(): Promise<void> {
    this.thinking.set(true);
    this.question.set('');
    this.answer.set('');
    this.audioUrl.set('');

    try {
      const wav = await toWav(new Blob(this.parts));
      const reply = await this.api.voiceAsk(wav);

      this.question.set(reply.question);
      this.answer.set(reply.answer);

      if (reply.audioBase64) {
        this.audioUrl.set(`data:audio/mpeg;base64,${reply.audioBase64}`);
      }
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen.');
    } finally {
      this.thinking.set(false);
    }
  }
}
