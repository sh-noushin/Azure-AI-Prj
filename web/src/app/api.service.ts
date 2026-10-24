import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

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
}
