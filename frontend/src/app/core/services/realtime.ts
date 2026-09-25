import { Injectable, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject, firstValueFrom } from 'rxjs';
import { AppNotification } from '../models/notification.models';
import {
  AccessChangedEvent,
  ActiveRowChangedEvent,
  CellsChangedEvent,
  PositionChangedEvent,
  PresenceChangedEvent,
  PresenceUser,
  WorkEvent,
} from '../models/realtime.models';
import { Auth } from './auth';

export type RealtimeState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

const RETRY_DELAY_MS = 8000;

function isExpiring(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))) as { exp?: number };
    return !payload.exp || payload.exp * 1000 - Date.now() < 30000;
  } catch {
    return true;
  }
}

@Injectable({
  providedIn: 'root',
})
export class Realtime {
  private readonly auth = inject(Auth);

  private connection: HubConnection | null = null;
  private retryTimer: ReturnType<typeof setTimeout> | null = null;
  private hasConnectedBefore = false;
  private readonly joined = new Set<string>();

  private readonly stateState = signal<RealtimeState>('disconnected');
  private readonly presenceState = signal<Record<string, PresenceUser[]>>({});

  readonly state = this.stateState.asReadonly();

  readonly cellsChanged$ = new Subject<CellsChangedEvent>();
  readonly positionChanged$ = new Subject<PositionChangedEvent>();
  readonly activeRowChanged$ = new Subject<ActiveRowChangedEvent>();
  readonly patternReset$ = new Subject<WorkEvent>();
  readonly commentsChanged$ = new Subject<WorkEvent>();
  readonly accessChanged$ = new Subject<AccessChangedEvent>();
  readonly notification$ = new Subject<AppNotification>();
  readonly reconnected$ = new Subject<void>();

  get connectionId(): string | null {
    const connection = this.connection;
    return connection && connection.state === HubConnectionState.Connected ? connection.connectionId : null;
  }

  usersOn(workId: string): PresenceUser[] {
    const selfId = this.auth.user()?.id;
    return (this.presenceState()[workId] ?? []).filter((user) => user.userId !== selfId);
  }

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/app', { accessTokenFactory: () => this.token() })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('CellsChanged', (event: CellsChangedEvent) => this.cellsChanged$.next(event));
    connection.on('PositionChanged', (event: PositionChangedEvent) => this.positionChanged$.next(event));
    connection.on('ActiveRowChanged', (event: ActiveRowChangedEvent) => this.activeRowChanged$.next(event));
    connection.on('PatternReset', (event: WorkEvent) => this.patternReset$.next(event));
    connection.on('CommentsChanged', (event: WorkEvent) => this.commentsChanged$.next(event));
    connection.on('AccessChanged', (event: AccessChangedEvent) => this.accessChanged$.next(event));
    connection.on('NotificationReceived', (event: AppNotification) => this.notification$.next(event));
    connection.on('PresenceChanged', (event: PresenceChangedEvent) =>
      this.presenceState.update((state) => ({ ...state, [event.workId]: event.users })),
    );

    connection.onreconnecting(() => this.stateState.set('reconnecting'));
    connection.onreconnected(async () => {
      this.stateState.set('connected');
      await this.rejoinAll();
      this.reconnected$.next();
    });
    connection.onclose(() => {
      this.stateState.set('disconnected');
      if (this.connection === connection) {
        this.scheduleRetry();
      }
    });

    this.connection = connection;
    await this.connect();
  }

  async stop(): Promise<void> {
    const connection = this.connection;
    this.connection = null;
    this.hasConnectedBefore = false;
    this.joined.clear();
    this.presenceState.set({});
    this.stateState.set('disconnected');

    if (this.retryTimer) {
      clearTimeout(this.retryTimer);
      this.retryTimer = null;
    }

    await connection?.stop();
  }

  async joinWork(workId: string): Promise<void> {
    this.joined.add(workId);
    await this.invokeJoin(workId);
  }

  async leaveWork(workId: string): Promise<void> {
    this.joined.delete(workId);
    this.presenceState.update((state) => {
      const { [workId]: _removed, ...rest } = state;
      return rest;
    });

    if (this.connection?.state === HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveWork', workId);
      } catch {
        return;
      }
    }
  }

  private async connect(): Promise<void> {
    const connection = this.connection;
    if (!connection) {
      return;
    }

    this.stateState.set('connecting');
    try {
      await connection.start();
      if (this.connection !== connection) {
        return;
      }
      this.stateState.set('connected');
      await this.rejoinAll();
      if (this.hasConnectedBefore) {
        this.reconnected$.next();
      }
      this.hasConnectedBefore = true;
    } catch {
      this.stateState.set('disconnected');
      this.scheduleRetry();
    }
  }

  private scheduleRetry(): void {
    if (this.retryTimer || !this.connection) {
      return;
    }
    this.retryTimer = setTimeout(() => {
      this.retryTimer = null;
      void this.connect();
    }, RETRY_DELAY_MS);
  }

  private async rejoinAll(): Promise<void> {
    for (const workId of [...this.joined]) {
      await this.invokeJoin(workId);
    }
  }

  private async invokeJoin(workId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke('JoinWork', workId);
    } catch {
      this.joined.delete(workId);
    }
  }

  private async token(): Promise<string> {
    const current = this.auth.accessToken;
    if (current && !isExpiring(current)) {
      return current;
    }

    try {
      await firstValueFrom(this.auth.refresh());
    } catch {
      return '';
    }
    return this.auth.accessToken ?? '';
  }
}
