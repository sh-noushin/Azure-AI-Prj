import { Routes } from '@angular/router';
import { Chat } from './chat/chat';
import { Embeddings } from './embeddings/embeddings';
import { Language } from './language/language';
import { Rag } from './rag/rag';
import { Speech } from './speech/speech';
import { Vision } from './vision/vision';

export const routes: Routes = [
  { path: '', redirectTo: 'chat', pathMatch: 'full' },
  { path: 'chat', component: Chat },
  { path: 'embeddings', component: Embeddings },
  { path: 'rag', component: Rag },
  { path: 'language', component: Language },
  { path: 'vision', component: Vision },
  { path: 'speech', component: Speech },
];
