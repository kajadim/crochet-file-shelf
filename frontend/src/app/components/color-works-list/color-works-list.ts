import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { YarnColorApi } from '../../core/api/yarn-color-api';
import { YarnColorWork } from '../../core/models/yarn-color.models';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-color-works-list',
  imports: [RouterLink, TranslocoPipe],
  templateUrl: './color-works-list.html',
  styleUrl: './color-works-list.scss',
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
