import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Toast } from 'primeng/toast';
import { TranslocoPipe } from '@jsverse/transloco';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NotificationBell } from '../../components/notification-bell/notification-bell';
import { Auth } from '../../core/services/auth';
import { Language } from '../../core/services/language';
import { NotificationStore } from '../../core/services/notification-store';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule, Toast, TranslocoPipe, NotificationBell],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout implements OnInit, OnDestroy {
  protected readonly auth = inject(Auth);
  protected readonly language = inject(Language);
  private readonly notifications = inject(NotificationStore);

  ngOnInit(): void {
    this.notifications.start();
  }

  ngOnDestroy(): void {
    this.notifications.stop();
  }

  protected onLanguageChange(event: Event): void {
    this.language.setLanguage((event.target as HTMLSelectElement).value);
  }
}
