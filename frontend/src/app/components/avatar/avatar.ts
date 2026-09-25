import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { AvatarCache } from '../../core/services/avatar-cache';

@Component({
  selector: 'app-avatar',
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss',
})
export class Avatar {
  readonly userId = input.required<string>();
  readonly name = input.required<string>();
  readonly version = input<number | null | undefined>(null);
  readonly size = input(32);

  private readonly cache = inject(AvatarCache);

  protected readonly url = signal<string | null>(null);
  protected readonly initials = computed(() => this.name().trim().slice(0, 2).toUpperCase());
  protected readonly tone = computed(() => {
    let hash = 0;
    for (const char of this.userId() + this.name()) {
      hash = (hash * 31 + char.charCodeAt(0)) % 997;
    }
    return hash % 4;
  });

  constructor() {
    effect((onCleanup) => {
      const userId = this.userId();
      const version = this.version();
      let cancelled = false;
      onCleanup(() => (cancelled = true));

      if (version == null) {
        this.url.set(null);
        return;
      }

      void this.cache.load(userId, version).then((url) => {
        if (!cancelled) {
          this.url.set(url);
        }
      });
    });
  }
}
