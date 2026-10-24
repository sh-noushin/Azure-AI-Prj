import { Component, input } from '@angular/core';

/** Shows an error, or nothing at all when there is none. */
@Component({
  selector: 'app-error-message',
  templateUrl: './error-message.html',
})
export class ErrorMessage {
  message = input('');
}
