import { Injectable, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { LANGUAGE_STORAGE_KEY, SUPPORTED_LANGUAGES } from '../i18n/languages';

@Injectable({ providedIn: 'root' })
export class Language {
  private readonly transloco = inject(TranslocoService);

  readonly options = SUPPORTED_LANGUAGES;
  readonly current = this.transloco.activeLang;

  setLanguage(code: string): void {
    if (!SUPPORTED_LANGUAGES.some((option) => option.code === code)) {
      return;
    }

    this.transloco.setActiveLang(code);

    try {
      localStorage.setItem(LANGUAGE_STORAGE_KEY, code);
    } catch {
      // ignore (private browsing, storage disabled)
    }
  }
}
