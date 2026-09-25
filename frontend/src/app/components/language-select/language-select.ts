import { Component, inject } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { Language } from '../../core/services/language';

@Component({
  selector: 'app-language-select',
  imports: [TranslocoPipe],
  host: { class: 'fixed top-4 right-4 z-10' },
  template: `
    <select
      class="rounded border border-gray-300 bg-white px-2 py-1.5 text-sm"
      [attr.aria-label]="'profile.language' | transloco"
      (change)="onChange($event)"
    >
      @for (option of language.options; track option.code) {
        <option [value]="option.code" [selected]="option.code === language.current()">{{ option.label }}</option>
      }
    </select>
  `,
})
export class LanguageSelect {
  protected readonly language = inject(Language);

  protected onChange(event: Event): void {
    this.language.setLanguage((event.target as HTMLSelectElement).value);
  }
}
