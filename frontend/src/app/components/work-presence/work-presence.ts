import { Component, OnDestroy, OnInit, computed, inject, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { PresenceUser } from '../../core/models/realtime.models';
import { Realtime } from '../../core/services/realtime';

const MAX_VISIBLE = 4;

@Component({
  selector: 'app-work-presence',
  imports: [TranslocoPipe],
  templateUrl: './work-presence.html',
  styleUrl: './work-presence.scss',
})
export class WorkPresence implements OnInit, OnDestroy {
  readonly workId = input.required<string>();

  private readonly realtime = inject(Realtime);

  protected readonly users = computed(() => this.realtime.usersOn(this.workId()));
  protected readonly visible = computed(() => this.users().slice(0, MAX_VISIBLE));
  protected readonly hiddenCount = computed(() => Math.max(0, this.users().length - MAX_VISIBLE));
  protected readonly names = computed(() => this.users().map((user) => user.displayName).join(', '));

  ngOnInit(): void {
    void this.realtime.joinWork(this.workId());
  }

  ngOnDestroy(): void {
    void this.realtime.leaveWork(this.workId());
  }

  protected initials(user: PresenceUser): string {
    return user.displayName.trim().slice(0, 2).toUpperCase();
  }
}
