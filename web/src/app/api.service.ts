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

/** One place for every call to the C# API. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/api';

  /** Ask the chat model a question. */
  chat(question: string): Promise<{ answer: string }> {
    return firstValueFrom(
      this.http.post<{ answer: string }>(`${this.baseUrl}/chat`, { question })
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
