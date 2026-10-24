import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** What the Language page gets back. */
export interface TextAnalysis {
  sentiment: string;
  positive: number;
  neutral: number;
  negative: number;
  language: string;
  keyPhrases: string[];
  entities: { text: string; category: string }[];
}

/** What the Vision page gets back. */
export interface VisionAnalysis {
  description: string;
  textLines: string[];
  tags: { name: string; confidence: number }[];
}

/** A voice the Speech service can use. */
export interface VoiceOption {
  id: string;
  label: string;
}

/** A language the Translator can translate into. */
export interface LanguageOption {
  code: string;
  label: string;
}

/** One message in a conversation. */
export interface ChatTurn {
  role: 'user' | 'assistant';
  text: string;
}

/** One place for every call to the C# API. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/api';

  /** Ask the chat model a question, sending the conversation so far as context. */
  chat(question: string, history: ChatTurn[] = []): Promise<{ answer: string }> {
    return firstValueFrom(
      this.http.post<{ answer: string }>(`${this.baseUrl}/chat`, { question, history })
    );
  }

  /** Compare two texts by meaning. */
  similarity(first: string, second: string): Promise<{ similarity: number; dimensions: number }> {
    return firstValueFrom(
      this.http.post<{ similarity: number; dimensions: number }>(
        `${this.baseUrl}/similarity`,
        { first, second }
      )
    );
  }

  /** Upload a document for the "ask your own file" page. */
  ragUpload(file: File): Promise<{ documentName: string; chunkCount: number }> {
    const form = new FormData();
    form.append('file', file);

    return firstValueFrom(
      this.http.post<{ documentName: string; chunkCount: number }>(
        `${this.baseUrl}/rag/upload`,
        form
      )
    );
  }

  /** Analyse a text: mood, language, topics and names. */
  language(text: string): Promise<TextAnalysis> {
    return firstValueFrom(
      this.http.post<TextAnalysis>(`${this.baseUrl}/language`, { text })
    );
  }

  /** Describe a picture, read its text and list what is in it. */
  vision(file: File): Promise<VisionAnalysis> {
    const form = new FormData();
    form.append('file', file);

    return firstValueFrom(this.http.post<VisionAnalysis>(`${this.baseUrl}/vision`, form));
  }

  /** The voices available for reading text out loud. */
  voices(): Promise<VoiceOption[]> {
    return firstValueFrom(this.http.get<VoiceOption[]>(`${this.baseUrl}/speech/voices`));
  }

  /** Read a text out loud and return the audio. */
  speak(text: string, voice: string): Promise<Blob> {
    return firstValueFrom(
      this.http.post(`${this.baseUrl}/speech/speak`, { text, voice }, { responseType: 'blob' })
    );
  }

  /** Write down what is said in a WAV file. */
  transcribe(file: File, language: string): Promise<{ text: string; language: string }> {
    const form = new FormData();
    form.append('file', file);

    return firstValueFrom(
      this.http.post<{ text: string; language: string }>(
        `${this.baseUrl}/speech/transcribe?language=${language}`,
        form
      )
    );
  }

  /** The languages available for translation. */
  translateLanguages(): Promise<LanguageOption[]> {
    return firstValueFrom(this.http.get<LanguageOption[]>(`${this.baseUrl}/translate/languages`));
  }

  /** Translate a text; the source language is detected automatically. */
  translate(text: string, to: string): Promise<{ text: string; detectedLanguage: string }> {
    return firstValueFrom(
      this.http.post<{ text: string; detectedLanguage: string }>(
        `${this.baseUrl}/translate`,
        { text, to }
      )
    );
  }

  /** Send a spoken question and get back the answer, in text and as audio. */
  voiceAsk(wav: Blob): Promise<{ question: string; answer: string; audioBase64: string }> {
    const form = new FormData();
    form.append('file', wav, 'frage.wav');

    return firstValueFrom(
      this.http.post<{ question: string; answer: string; audioBase64: string }>(
        `${this.baseUrl}/voice/ask?language=de-DE&voice=de-DE-KatjaNeural`,
        form
      )
    );
  }

  /** Ask a question about the uploaded document. */
  ragAsk(question: string): Promise<{ answer: string; sources: string[] }> {
    return firstValueFrom(
      this.http.post<{ answer: string; sources: string[] }>(
        `${this.baseUrl}/rag/ask`,
        { question }
      )
    );
  }
}
