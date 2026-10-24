import { Component, input } from '@angular/core';

/** Shows the result of an Azure AI call. Used by every feature page. */
@Component({
  selector: 'app-answer-box',
  templateUrl: './answer-box.html',
})
export class AnswerBox {
  label = input('Antwort');
  text = input('');
}
