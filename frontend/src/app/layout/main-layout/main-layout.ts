import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Toast } from 'primeng/toast';
import { TranslocoPipe } from '@jsverse/transloco';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from '../../core/services/auth';
import { Language } from '../../core/services/language';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule, Toast, TranslocoPipe],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout {
  protected readonly auth = inject(Auth);
  protected readonly language = inject(Language);

  protected onLanguageChange(event: Event): void {
    this.language.setLanguage((event.target as HTMLSelectElement).value);
  }
}
