import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { SiteApi } from '../api/site-api';
import { Site } from '../models/site.models';
import { extractErrorMessage } from '../utils/http-error';

@Injectable({
  providedIn: 'root',
})
export class SiteStore {
  private readonly api = inject(SiteApi);

  private readonly siteState = signal<Site | null>(null);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  private workId: string | null = null;

  readonly site = this.siteState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  reset(): void {
    this.workId = null;
    this.siteState.set(null);
    this.loadingState.set(false);
    this.errorState.set(null);
  }

  load(workId: string): void {
    this.workId = workId;
    this.loadingState.set(true);
    this.errorState.set(null);

    this.api.get(workId).subscribe({
      next: (site) => {
        this.siteState.set(site);
        this.loadingState.set(false);
      },
      error: (error) => {
        this.errorState.set(extractErrorMessage(error));
        this.loadingState.set(false);
      },
    });
  }

  update(url: string): Observable<Site> {
    return this.api.update(this.workId!, { url }).pipe(tap((site) => this.siteState.set(site)));
  }
}
