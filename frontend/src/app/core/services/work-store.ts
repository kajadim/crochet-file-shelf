import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { WorkApi } from '../api/work-api';
import { CreateWorkRequest, UpdateWorkRequest, Work, WorkQuery } from '../models/work.models';
import { extractErrorMessage } from '../utils/http-error';

@Injectable({
  providedIn: 'root',
})
export class WorkStore {
  private readonly api = inject(WorkApi);

  private readonly worksState = signal<Work[]>([]);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  private currentQuery: WorkQuery = {};
  private requestId = 0;

  readonly works = this.worksState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  reset(): void {
    this.requestId++;
    this.currentQuery = {};
    this.worksState.set([]);
    this.loadingState.set(false);
    this.errorState.set(null);
  }

  load(query: WorkQuery): void {
    const requestId = ++this.requestId;
    this.currentQuery = query;
    this.loadingState.set(true);
    this.errorState.set(null);

    this.api.getAll(query).subscribe({
      next: (works) => {
        if (requestId !== this.requestId) {
          return;
        }
        this.worksState.set(this.sorted(works));
        this.loadingState.set(false);
      },
      error: (error) => {
        if (requestId !== this.requestId) {
          return;
        }
        this.errorState.set(extractErrorMessage(error));
        this.loadingState.set(false);
      },
    });
  }

  create(request: CreateWorkRequest): Observable<Work> {
    return this.api.create(request).pipe(
      tap((work) => {
        if (this.isFiltered()) {
          this.load(this.currentQuery);
        } else if (this.isVisibleInCurrentView(work)) {
          this.worksState.update((works) => this.sorted([...works, work]));
        }
      }),
    );
  }

  update(id: string, request: UpdateWorkRequest): Observable<Work> {
    return this.api.update(id, request).pipe(tap((work) => this.replace(work)));
  }

  move(id: string, folderId: string): Observable<Work> {
    return this.api.move(id, { folderId }).pipe(
      tap((work) => {
        if (this.isFiltered() || this.isVisibleInCurrentView(work)) {
          this.replace(work);
        } else {
          this.worksState.update((works) => works.filter((existing) => existing.id !== id));
        }
      }),
    );
  }

  remove(id: string): Observable<void> {
    return this.api
      .delete(id)
      .pipe(tap(() => this.worksState.update((works) => works.filter((work) => work.id !== id))));
  }

  reload(): void {
    this.load(this.currentQuery);
  }

  dropLocal(id: string): void {
    this.worksState.update((works) => works.filter((work) => work.id !== id));
  }

  private isFiltered(): boolean {
    const query = this.currentQuery;
    return !!(query.search || query.type || query.colorId || query.platform);
  }

  private isVisibleInCurrentView(work: Work): boolean {
    if (this.currentQuery.shared) {
      return false;
    }
    const folderId = this.currentQuery.folderId ?? null;
    return folderId === null || folderId === work.folderId;
  }

  private replace(updated: Work): void {
    this.worksState.update((works) => this.sorted(works.map((work) => (work.id === updated.id ? updated : work))));
  }

  private sorted(works: Work[]): Work[] {
    return [...works].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
  }
}
