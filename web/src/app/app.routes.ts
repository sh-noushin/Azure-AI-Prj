import { Routes } from '@angular/router';
import { Chat } from './chat/chat';
import { Embeddings } from './embeddings/embeddings';

export const routes: Routes = [
  { path: '', redirectTo: 'chat', pathMatch: 'full' },
  { path: 'chat', component: Chat },
  { path: 'embeddings', component: Embeddings },
];
