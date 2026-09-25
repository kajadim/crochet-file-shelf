import { Injectable, inject, signal } from '@angular/core';
import { NotificationApi } from '../api/notification-api';
import { AppNotification } from '../models/notification.models';
import { Realtime } from './realtime';

const MAX_ITEMS = 50;

@Injectable({
  providedIn: 'root',
})
export class NotificationStore {
  private readonly api = inject(NotificationApi);
  private readonly realtime = inject(Realtime);

  private readonly itemsState = signal<AppNotification[]>([]);
  private readonly unreadState = signal(0);

  readonly items = this.itemsState.asReadonly();
  readonly unreadCount = this.unreadState.asReadonly();

  constructor() {
    this.realtime.notification$.subscribe((notification) => this.receive(notification));
    this.realtime.reconnected$.subscribe(() => this.refresh());
  }

  start(): void {
    this.refresh();
  }

  stop(): void {
    this.itemsState.set([]);
    this.unreadState.set(0);
  }

  refresh(): void {
    this.api.getAll().subscribe({
      next: (list) => {
        this.itemsState.set(list.items);
        this.unreadState.set(list.unreadCount);
      },
      error: () => undefined,
    });
  }

  markRead(notification: AppNotification): void {
    if (notification.isRead) {
      return;
    }

    this.itemsState.update((items) => items.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item)));
    this.unreadState.update((count) => Math.max(0, count - 1));
    this.api.markRead(notification.id).subscribe({ error: () => this.refresh() });
  }

  markAllRead(): void {
    this.itemsState.update((items) => items.map((item) => ({ ...item, isRead: true })));
    this.unreadState.set(0);
    this.api.markAllRead().subscribe({ error: () => this.refresh() });
  }

  remove(notification: AppNotification): void {
    this.itemsState.update((items) => items.filter((item) => item.id !== notification.id));
    if (!notification.isRead) {
      this.unreadState.update((count) => Math.max(0, count - 1));
    }
    this.api.delete(notification.id).subscribe({ error: () => this.refresh() });
  }

  private receive(notification: AppNotification): void {
    if (this.itemsState().some((item) => item.id === notification.id)) {
      return;
    }

    this.itemsState.update((items) => [notification, ...items].slice(0, MAX_ITEMS));
    if (!notification.isRead) {
      this.unreadState.update((count) => count + 1);
    }
  }
}
