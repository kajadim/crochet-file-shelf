import { DatePipe } from '@angular/common';
import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { WorkApi } from '../../core/api/work-api';
import { AppNotification } from '../../core/models/notification.models';
import { NotificationStore } from '../../core/services/notification-store';

interface NotificationData {
  workName?: string;
  permission?: string;
  userName?: string;
}

const LINK_MARKER = '\u0001';
const LINKABLE_TYPES = ['WorkInvite', 'MemberJoined', 'OwnershipTransferred', 'CommentAdded'];

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
  private readonly transloco = inject(TranslocoService);

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
      case 'CommentAdded':
        return { key: 'notifications.commentAdded', params };
      case 'RemovedFromWork':
        return { key: 'notifications.removedFromWork', params };
      case 'WorkDeleted':
        return { key: 'notifications.workDeleted', params };
      default:
        return { key: 'notifications.ownershipTransferred', params };
    }
  }

  protected segments(notification: AppNotification): { before: string; name: string; after: string } | null {
    const info = this.describe(notification);
    if (!notification.workId || !LINKABLE_TYPES.includes(notification.type) || !info.params['workName']) {
      return null;
    }

    const text = this.transloco.translate(info.key, { ...info.params, workName: LINK_MARKER });
    const index = text.indexOf(LINK_MARKER);
    if (index < 0) {
      return null;
    }

    return {
      before: text.slice(0, index),
      name: info.params['workName'],
      after: text.slice(index + LINK_MARKER.length),
    };
  }

  protected iconFor(notification: AppNotification): string {
    switch (notification.type) {
      case 'WorkInvite':
      case 'MemberJoined':
        return 'pi-user-plus';
      case 'OwnershipTransferred':
        return 'pi-key';
      case 'CommentAdded':
        return 'pi-comment';
      default:
        return 'pi-user-minus';
    }
  }

  protected onLinkClick(event: MouseEvent, notification: AppNotification): void {
    event.stopPropagation();
    this.onItemClick(notification);
  }

  protected onItemClick(notification: AppNotification): void {
    this.store.markRead(notification);

    if (!notification.workId || !LINKABLE_TYPES.includes(notification.type)) {
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
