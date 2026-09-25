import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { CommentApi } from '../api/comment-api';
import { WorkComment } from '../models/comment.models';
import { extractErrorMessage } from '../utils/http-error';
import { Realtime } from './realtime';

function lastActivity(comment: WorkComment): number {
  return new Date(comment.updatedAt ?? comment.createdAt).getTime();
}

function sortByLastActivity(comments: WorkComment[]): WorkComment[] {
  return [...comments].sort((a, b) => lastActivity(b) - lastActivity(a));
}

@Injectable({
  providedIn: 'root',
})
export class CommentStore {
  private readonly api = inject(CommentApi);
  private readonly realtime = inject(Realtime);

  private readonly commentsState = signal<WorkComment[]>([]);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  private workId: string | null = null;
  private requestId = 0;

  readonly comments = this.commentsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  constructor() {
    this.realtime.commentsChanged$.subscribe((event) => this.refresh(event.workId));
    this.realtime.reconnected$.subscribe(() => this.refresh(this.workId));
  }

  reset(): void {
    this.requestId++;
    this.workId = null;
    this.commentsState.set([]);
    this.loadingState.set(false);
    this.errorState.set(null);
  }

  load(workId: string): void {
    const requestId = ++this.requestId;
    this.workId = workId;
    this.loadingState.set(true);
    this.errorState.set(null);

    this.api.getAll(workId).subscribe({
      next: (comments) => {
        if (requestId !== this.requestId) {
          return;
        }
        this.commentsState.set(sortByLastActivity(comments));
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

  private refresh(workId: string | null): void {
    if (!workId || workId !== this.workId) {
      return;
    }

    this.api.getAll(workId).subscribe({
      next: (comments) => {
        if (workId === this.workId) {
          this.commentsState.set(sortByLastActivity(comments));
        }
      },
      error: () => undefined,
    });
  }

  create(text: string): Observable<WorkComment> {
    return this.api
      .create(this.workId!, { text })
      .pipe(tap((comment) => this.commentsState.update((comments) => sortByLastActivity([comment, ...comments]))));
  }

  update(commentId: string, text: string): Observable<WorkComment> {
    return this.api.update(this.workId!, commentId, { text }).pipe(
      tap((updated) =>
        this.commentsState.update((comments) =>
          sortByLastActivity(comments.map((comment) => (comment.id === commentId ? updated : comment))),
        ),
      ),
    );
  }

  remove(commentId: string): Observable<void> {
    return this.api
      .delete(this.workId!, commentId)
      .pipe(tap(() => this.commentsState.update((comments) => comments.filter((comment) => comment.id !== commentId))));
  }
}
