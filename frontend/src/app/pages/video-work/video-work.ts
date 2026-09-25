import { Component, DestroyRef, ElementRef, OnInit, computed, effect, inject, signal, untracked, viewChild } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { InputTextModule } from 'primeng/inputtext';
import { WorkComments } from '../../components/work-comments/work-comments';
import { WorkPresence } from '../../components/work-presence/work-presence';
import { WorkAccessWatcher } from '../../core/services/work-access-watcher';
import { WorkApi } from '../../core/api/work-api';
import { WorkRole } from '../../core/models/work.models';
import { VideoStore } from '../../core/services/video-store';
import { YouTubePlayerService } from '../../core/services/youtube-player';
import { extractErrorMessage } from '../../core/utils/http-error';
import { formatTimestamp, splitTimestamp } from '../../core/utils/time';

@Component({
  selector: 'app-video-work',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, DrawerModule, InputTextModule, TranslocoPipe, WorkComments, WorkPresence],
  templateUrl: './video-work.html',
  styleUrl: './video-work.scss',
})
export class VideoWork implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly workApi = inject(WorkApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly watcher = inject(WorkAccessWatcher);
  private readonly destroyRef = inject(DestroyRef);
  private readonly transloco = inject(TranslocoService);
  private readonly youtube = inject(YouTubePlayerService);

  protected readonly videoStore = inject(VideoStore);

  protected readonly workId = this.route.snapshot.paramMap.get('workId')!;
  protected readonly workName = signal<string | null>(null);
  protected readonly role = signal<WorkRole | null>(null);
  protected readonly canEdit = computed(() => this.role() === 'Owner' || this.role() === 'Editor');
  protected readonly isOwner = computed(() => this.role() === 'Owner');

  protected commentsOpen = false;

  private readonly player = viewChild<ElementRef<HTMLIFrameElement>>('player');
  protected readonly duration = signal<number | null>(null);

  protected readonly embedSources = computed(() => {
    const video = this.videoStore.video();
    if (!video?.embedUrl) {
      return [];
    }

    let url = video.embedUrl;
    if (video.platform === 'YouTube') {
      url += (url.includes('?') ? '&' : '?') + 'enablejsapi=1&origin=' + encodeURIComponent(window.location.origin);
    }
    return [{ raw: url, safe: this.sanitizer.bypassSecurityTrustResourceUrl(url) }];
  });

  protected readonly frameClass = computed(() => {
    switch (this.videoStore.video()?.platform) {
      case 'YouTube':
        return 'aspect-video w-full max-w-5xl';
      case 'TikTok':
        return 'h-[740px] w-full max-w-[340px]';
      case 'Instagram':
        return 'h-[720px] w-full max-w-[540px]';
      default:
        return 'h-[520px] w-full max-w-[420px]';
    }
  });

  protected readonly linkForm = this.fb.group({
    url: ['', [Validators.required, Validators.maxLength(2048), Validators.pattern(/\S/)]],
  });
  protected readonly linkSaving = signal(false);
  protected readonly linkError = signal<string | null>(null);

  protected readonly timeForm = this.fb.group({
    hours: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(99)]),
    minutes: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(59)]),
    seconds: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(59)]),
  });
  protected readonly timeSaving = signal(false);
  protected readonly timeError = signal<string | null>(null);

  constructor() {
    effect(() => {
      const video = this.videoStore.video();
      if (video) {
        untracked(() => {
          this.linkForm.patchValue({ url: video.originalUrl });
          this.timeForm.reset(
            video.timestampSeconds === null
              ? { hours: null, minutes: null, seconds: null }
              : splitTimestamp(video.timestampSeconds),
          );
        });
      }
    });

    effect(() => {
      const element = this.player()?.nativeElement;
      untracked(() => this.duration.set(null));
      if (element && untracked(() => this.videoStore.video()?.platform) === 'YouTube') {
        this.youtube.getDuration(element).then((duration) => {
          if (this.player()?.nativeElement === element) {
            this.duration.set(duration);
          }
        });
      }
    });
  }

  ngOnInit(): void {
    const access = this.watcher.watch(this.workId, (role) => this.role.set(role));
    this.destroyRef.onDestroy(() => access.unsubscribe());

    this.videoStore.reset();
    this.videoStore.load(this.workId);
    this.workApi.getById(this.workId).subscribe({
      next: (work) => {
        this.workName.set(work.name);
        this.role.set(work.role);
      },
    });
  }

  protected openOriginal(url: string): void {
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  protected saveLink(): void {
    if (this.linkForm.invalid) {
      this.linkForm.markAllAsTouched();
      return;
    }

    this.linkSaving.set(true);
    this.linkError.set(null);

    this.videoStore.updateLink(this.linkForm.getRawValue().url.trim()).subscribe({
      next: () => this.linkSaving.set(false),
      error: (err) => {
        this.linkError.set(extractErrorMessage(err));
        this.linkSaving.set(false);
      },
    });
  }

  protected saveTimestamp(): void {
    if (this.timeForm.invalid) {
      this.timeForm.markAllAsTouched();
      return;
    }

    const { hours, minutes, seconds: secs } = this.timeForm.getRawValue();
    const total = (hours ?? 0) * 3600 + (minutes ?? 0) * 60 + (secs ?? 0);
    const seconds = total > 0 ? total : null;

    const duration = this.duration();
    if (seconds !== null && duration !== null && seconds >= duration) {
      this.timeError.set(this.transloco.translate('videoWork.startTimeTooLong', { duration: formatTimestamp(duration) }));
      return;
    }

    this.timeSaving.set(true);
    this.timeError.set(null);

    this.videoStore.updateTimestamp(seconds).subscribe({
      next: () => this.timeSaving.set(false),
      error: (err) => {
        this.timeError.set(extractErrorMessage(err));
        this.timeSaving.set(false);
      },
    });
  }
}
