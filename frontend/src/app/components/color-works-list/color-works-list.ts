import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { YarnColorApi } from '../../core/api/yarn-color-api';
import { YarnColorWork } from '../../core/models/yarn-color.models';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-color-works-list',
  imports: [RouterLink, TranslocoPipe],
  template: `
    @if (loading()) {
      <p class="text-sm text-gray-500">{{ 'common.loading' | transloco }}</p>
    } @else if (error()) {
      <p class="rounded bg-red-50 px-3 py-2 text-sm text-red-800">{{ error() }}</p>
    } @else if (works().length === 0) {
      <p class="text-sm text-gray-500">{{ 'palette.notUsedYet' | transloco }}</p>
    } @else {
      <ul class="flex flex-col">
        @for (work of works(); track work.id) {
          <li>
            <a
              [routerLink]="['/works', work.id, 'matrix']"
              class="flex items-center gap-2 rounded px-2 py-1.5 text-sm text-blue-700 hover:bg-gray-100 hover:underline"
              (click)="opened.emit()"
            >
              <i class="pi pi-table text-xs text-gray-500"></i>
              <span class="truncate">{{ work.name }}</span>
            </a>
          </li>
        }
      </ul>
    }
  `,
})
export class ColorWorksList implements OnInit {
  readonly colorId = input.required<string>();
  readonly opened = output<void>();

  private readonly api = inject(YarnColorApi);

  protected readonly works = signal<YarnColorWork[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.getWorks(this.colorId()).subscribe({
      next: (works) => {
        this.works.set(works);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
