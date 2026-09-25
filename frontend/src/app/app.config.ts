import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  isDevMode,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import { TranslocoService, provideTransloco } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';

import { CrochetTheme } from './core/theme/crochet-theme';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { connectionIdInterceptor } from './core/interceptors/connection-id-interceptor';
import { languageInterceptor } from './core/interceptors/language-interceptor';
import { Auth } from './core/services/auth';
import { Language } from './core/services/language';
import { Theme } from './core/services/theme';
import { TranslocoHttpLoader } from './core/i18n/transloco-http-loader';
import { SUPPORTED_LANGUAGES, getInitialLanguage } from './core/i18n/languages';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([languageInterceptor, connectionIdInterceptor, authInterceptor])),
    provideTransloco({
      config: {
        availableLangs: SUPPORTED_LANGUAGES.map((option) => option.code),
        defaultLang: getInitialLanguage(),
        fallbackLang: 'en',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    provideAppInitializer(() => {
      const transloco = inject(TranslocoService);
      return firstValueFrom(transloco.load(transloco.getActiveLang()));
    }),
    provideAppInitializer(() => {
      inject(Theme);
      inject(Language);
    }),
    provideAppInitializer(() => inject(Auth).restoreSession()),
    provideAnimationsAsync(),
    providePrimeNG({ theme: { preset: CrochetTheme, options: { darkModeSelector: '[data-theme="dark"]' } } }),
    DialogService,
    MessageService,
  ],
};
