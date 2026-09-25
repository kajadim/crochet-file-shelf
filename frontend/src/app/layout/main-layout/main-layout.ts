import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Toast } from 'primeng/toast';
import { TranslocoPipe } from '@jsverse/transloco';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Avatar } from '../../components/avatar/avatar';
import { NotificationBell } from '../../components/notification-bell/notification-bell';
import { Auth } from '../../core/services/auth';
import { NotificationStore } from '../../core/services/notification-store';
import { Realtime } from '../../core/services/realtime';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Toast, TranslocoPipe, NotificationBell, Avatar],
  host: { class: 'flex h-dvh flex-col overflow-hidden' },
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout implements OnInit, OnDestroy {
  protected readonly auth = inject(Auth);
  private readonly notifications = inject(NotificationStore);
  protected readonly realtime = inject(Realtime);

  ngOnInit(): void {
    void this.realtime.start();
    this.notifications.start();
  }

  ngOnDestroy(): void {
    this.notifications.stop();
    void this.realtime.stop();
  }

}
