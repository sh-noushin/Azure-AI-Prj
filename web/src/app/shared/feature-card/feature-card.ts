import { Component, input } from '@angular/core';

/** The white box every feature page sits in: a title, a hint and the content. */
@Component({
  selector: 'app-feature-card',
  templateUrl: './feature-card.html',
  styleUrl: './feature-card.css',
})
export class FeatureCard {
  title = input.required<string>();
  hint = input('');
}
