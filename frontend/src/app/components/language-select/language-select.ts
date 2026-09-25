import { Component, inject } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { Language } from '../../core/services/language';

@Component({
  selector: 'app-language-select',
  imports: [TranslocoPipe],
  templateUrl: './language-select.html',
  styleUrl: './language-select.scss',
})
export class LanguageSelect {
  protected readonly language = inject(Language);

  protected onChange(event: Event): void {
    this.language.setLanguage((event.target as HTMLSelectElement).value);
  }
}
