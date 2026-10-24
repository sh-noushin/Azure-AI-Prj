import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

/** The top navigation. One link per Azure AI feature. */
@Component({
  selector: 'app-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
})
export class Nav {
  readonly links = [
    { path: '/chat', label: 'Chat' },
    { path: '/embeddings', label: 'Ähnlichkeit' },
    { path: '/rag', label: 'Eigenes Dokument' },
    { path: '/language', label: 'Textanalyse' },
    { path: '/vision', label: 'Bildanalyse' },
    { path: '/speech', label: 'Sprache' },
    { path: '/translate', label: 'Übersetzen' },
    { path: '/voice', label: 'Sprachassistent' },
  ];
}
