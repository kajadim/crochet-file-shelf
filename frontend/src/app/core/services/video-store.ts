import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { VideoApi } from '../api/video-api';
import { Video } from '../models/video.models';
import { extractErrorMessage } from '../utils/http-error';

@Injectable({
  providedIn: 'root',
})
export class VideoStore {
  private readonly api = inject(VideoApi);

  private readonly videoState = signal<Video | null>(null);
  private readonly availableState = signal<boolean | null>(null);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  private workId: string | null = null;

  readonly video = this.videoState.asReadonly();
  readonly available = this.availableState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  reset(): void {
    this.workId = null;
    this.videoState.set(null);
    this.availableState.set(null);
    this.loadingState.set(false);
    this.errorState.set(null);
  }

  load(workId: string): void {
    this.workId = workId;
    this.loadingState.set(true);
    this.errorState.set(null);

    this.api.get(workId).subscribe({
      next: (video) => {
        this.videoState.set(video);
        this.loadingState.set(false);
        this.loadStatus();
      },
      error: (error) => {
        this.errorState.set(extractErrorMessage(error));
        this.loadingState.set(false);
      },
    });
  }

  updateLink(url: string): Observable<Video> {
    return this.api.updateLink(this.workId!, { url }).pipe(
      tap((video) => {
        this.videoState.set(video);
        this.loadStatus();
      }),
    );
  }

  updateTimestamp(seconds: number | null): Observable<Video> {
    return this.api.updateTimestamp(this.workId!, { seconds }).pipe(tap((video) => this.videoState.set(video)));
  }

  private loadStatus(): void {
    if (!this.workId) {
      return;
    }

    this.availableState.set(null);
    this.api.getStatus(this.workId).subscribe({
      next: (status) => this.availableState.set(status.available),
      error: () => this.availableState.set(null),
    });
  }
}
