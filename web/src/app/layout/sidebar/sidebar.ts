import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

/** One entry in the side menu. */
interface MenuItem {
  path: string;
  label: string;
  service: string;
  /** SVG path data for the icon, drawn as outlines. */
  icon: string[];
}

/** The side menu. One entry per Azure AI feature. */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  readonly items: MenuItem[] = [
    {
      path: '/chat',
      label: 'Chat',
      service: 'gpt-5-mini',
      icon: ['M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z'],
    },
    {
      path: '/embeddings',
      label: 'Ähnlichkeit',
      service: 'Embeddings',
      icon: ['M9 17H7A5 5 0 0 1 7 7h2', 'M15 7h2a5 5 0 1 1 0 10h-2', 'M8 12h8'],
    },
    {
      path: '/rag',
      label: 'Eigenes Dokument',
      service: 'Embeddings + Chat',
      icon: ['M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z', 'M14 2v6h6', 'M16 13H8', 'M16 17H8'],
    },
    {
      path: '/language',
      label: 'Textanalyse',
      service: 'Language',
      icon: ['M4 7V4h16v3', 'M9 20h6', 'M12 4v16'],
    },
    {
      path: '/vision',
      label: 'Bildanalyse',
      service: 'Vision',
      icon: ['M3 3h18v18H3z', 'M8.5 10a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z', 'M21 15l-5-5L5 21'],
    },
    {
      path: '/speech',
      label: 'Sprache',
      service: 'Speech',
      icon: ['M11 5 6 9H2v6h4l5 4z', 'M19.07 4.93a10 10 0 0 1 0 14.14', 'M15.54 8.46a5 5 0 0 1 0 7.07'],
    },
    {
      path: '/translate',
      label: 'Übersetzen',
      service: 'Translator',
      icon: ['m5 8 6 6', 'm4 14 6-6 2-3', 'M2 5h12', 'M7 2h1', 'm22 22-5-10-5 10', 'M14 18h6'],
    },
    {
      path: '/voice',
      label: 'Sprachassistent',
      service: 'Speech + Chat',
      icon: ['M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3z', 'M19 10v2a7 7 0 0 1-14 0v-2', 'M12 19v3'],
    },
  ];
}
