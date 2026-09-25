import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { AvatarCache } from '../../core/services/avatar-cache';

@Component({
  selector: 'app-avatar',
  template: `
    <span
      class="inline-flex shrink-0 items-center justify-center overflow-hidden rounded-full bg-teal-600 font-semibold text-white select-none"
      [style.width.px]="size()"
      [style.height.px]="size()"
      [style.font-size.px]="size() * 0.38"
      aria-hidden="true"
    >
      @if (url(); as src) {
        <img [src]="src" alt="" class="h-full w-full object-cover" />
      } @else {
        {{ initials() }}
      }
    </span>
  `,
})
export class Avatar {
  readonly userId = input.required<string>();
  readonly name = input.required<string>();
  readonly version = input<number | null | undefined>(null);
  readonly size = input(32);

  private readonly cache = inject(AvatarCache);

  protected readonly url = signal<string | null>(null);
  protected readonly initials = computed(() => this.name().trim().slice(0, 2).toUpperCase());

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
