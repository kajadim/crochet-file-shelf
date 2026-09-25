import { DatePipe } from '@angular/common';
import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { WorkApi } from '../../core/api/work-api';
import { AppNotification } from '../../core/models/notification.models';
import { NotificationStore } from '../../core/services/notification-store';

interface NotificationData {
  workName?: string;
  permission?: string;
  userName?: string;
}

@Component({
  selector: 'app-notification-bell',
  imports: [DatePipe, TranslocoPipe],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss',
})
export class NotificationBell {
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly router = inject(Router);
  private readonly workApi = inject(WorkApi);

  protected readonly store = inject(NotificationStore);
  protected readonly open = signal(false);

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.elementRef.nativeElement.contains(event.target as Node)) {
      this.open.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.open.set(false);
  }

  protected toggle(): void {
    const next = !this.open();
    this.open.set(next);
    if (next) {
      this.store.refresh();
    }
  }

  protected describe(notification: AppNotification): { key: string; params: Record<string, string> } {
    let data: NotificationData = {};
    try {
      data = JSON.parse(notification.message) as NotificationData;
    } catch {
      data = {};
    }

    const params = { workName: data.workName ?? '', userName: data.userName ?? '' };

    switch (notification.type) {
      case 'WorkInvite':
        return {
          key: data.permission === 'CanEdit' ? 'notifications.workInviteCanEdit' : 'notifications.workInviteViewOnly',
          params,
        };
      case 'MemberJoined':
        return { key: 'notifications.memberJoined', params };
      case 'RemovedFromWork':
        return { key: 'notifications.removedFromWork', params };
      case 'WorkDeleted':
        return { key: 'notifications.workDeleted', params };
      default:
        return { key: 'notifications.ownershipTransferred', params };
    }
  }

  protected iconFor(notification: AppNotification): string {
    switch (notification.type) {
      case 'WorkInvite':
      case 'MemberJoined':
        return 'pi-user-plus';
      case 'OwnershipTransferred':
        return 'pi-key';
      default:
        return 'pi-user-minus';
    }
  }

  protected onItemClick(notification: AppNotification): void {
    this.store.markRead(notification);

    const opensWork =
      notification.workId &&
      (notification.type === 'WorkInvite' ||
        notification.type === 'MemberJoined' ||
        notification.type === 'OwnershipTransferred');
    if (!opensWork) {
      return;
    }

    this.workApi.getById(notification.workId!).subscribe({
      next: (work) => {
        const routes = { Pattern: 'matrix', Video: 'video', Site: 'site' };
        this.open.set(false);
        this.router.navigate(['/works', work.id, routes[work.type]]);
      },
      error: () => undefined,
    });
  }

  protected remove(event: MouseEvent, notification: AppNotification): void {
    event.stopPropagation();
    this.store.remove(notification);
  }
}
