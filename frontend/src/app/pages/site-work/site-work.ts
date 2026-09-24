import { Component, OnInit, computed, effect, inject, signal, untracked } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { WorkComments } from '../../components/work-comments/work-comments';
import { WorkApi } from '../../core/api/work-api';
import { SiteStore } from '../../core/services/site-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-site-work',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, TranslocoPipe, WorkComments],
  templateUrl: './site-work.html',
  styleUrl: './site-work.scss',
})
export class SiteWork implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly workApi = inject(WorkApi);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly siteStore = inject(SiteStore);

  protected readonly workId = this.route.snapshot.paramMap.get('workId')!;
  protected readonly workName = signal<string | null>(null);
  protected readonly workDescription = signal<string | null>(null);

  protected readonly domain = computed(() => {
    const url = this.siteStore.site()?.url;
    if (!url) {
      return '';
    }
    try {
      return new URL(url).hostname.replace(/^www\./, '');
    } catch {
      return url;
    }
  });

  protected readonly linkForm = this.fb.group({
    url: ['', [Validators.required, Validators.maxLength(2048), Validators.pattern(/\S/)]],
  });
  protected readonly saving = signal(false);
  protected readonly linkError = signal<string | null>(null);

  constructor() {
    effect(() => {
      const site = this.siteStore.site();
      if (site) {
        untracked(() => this.linkForm.patchValue({ url: site.url }));
      }
    });
  }

  ngOnInit(): void {
    this.siteStore.reset();
    this.siteStore.load(this.workId);
    this.workApi.getById(this.workId).subscribe({
      next: (work) => {
        this.workName.set(work.name);
        this.workDescription.set(work.description);
      },
    });
  }

  protected openSite(url: string): void {
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  protected saveLink(): void {
    if (this.linkForm.invalid) {
      this.linkForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.linkError.set(null);

    this.siteStore.update(this.linkForm.getRawValue().url.trim()).subscribe({
      next: () => this.saving.set(false),
      error: (err) => {
        this.linkError.set(extractErrorMessage(err));
        this.saving.set(false);
      },
    });
  }
}
