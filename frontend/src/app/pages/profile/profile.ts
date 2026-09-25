import { DatePipe } from '@angular/common';
import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { filter, take } from 'rxjs';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Avatar } from '../../components/avatar/avatar';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { AuthApi } from '../../core/api/auth-api';
import { ProfileApi } from '../../core/api/profile-api';
import { Profile, ProfileSharedWork } from '../../core/models/profile.models';
import { Auth } from '../../core/services/auth';
import { Language } from '../../core/services/language';
import { Theme, ThemeMode } from '../../core/services/theme';
import { cropToAvatar } from '../../core/utils/avatar-image';
import { extractErrorMessage } from '../../core/utils/http-error';
import { USERNAME_PATTERN, usernameAvailableValidator } from '../../core/utils/username';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, ReactiveFormsModule, ButtonModule, InputTextModule, Textarea, TranslocoPipe, Avatar],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class ProfilePage implements OnInit {
  private readonly api = inject(ProfileApi);
  private readonly authApi = inject(AuthApi);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly dialogService = inject(DialogService);
  private readonly transloco = inject(TranslocoService);

  protected readonly auth = inject(Auth);
  protected readonly language = inject(Language);
  protected readonly theme = inject(Theme);

  protected readonly profile = signal<Profile | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly resetBusy = signal(false);
  protected readonly resetError = signal<string | null>(null);
  protected readonly editing = signal(false);
  protected readonly saving = signal(false);
  protected readonly editError = signal<string | null>(null);
  protected readonly avatarBusy = signal(false);
  protected readonly avatarError = signal<string | null>(null);

  protected readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    username: [
      '',
      [Validators.required, Validators.pattern(USERNAME_PATTERN)],
      [usernameAvailableValidator(this.authApi, () => this.profile()?.username ?? null)],
    ],
    bio: ['', [Validators.maxLength(500)]],
  });

  protected readonly totalWorks = computed(() => {
    const counts = this.profile()?.workCounts;
    return counts ? counts.pattern + counts.video + counts.site : 0;
  });

  protected readonly tiles = computed(() => {
    const counts = this.profile()?.workCounts;
    return [
      { key: 'workCard.matrix', icon: 'pi-table', count: counts?.pattern ?? 0 },
      { key: 'workCard.video', icon: 'pi-play-circle', count: counts?.video ?? 0 },
      { key: 'workCard.site', icon: 'pi-globe', count: counts?.site ?? 0 },
    ];
  });

  @HostListener('window:beforeunload', ['$event'])
  protected onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.editing() && this.form.dirty) {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  ngOnInit(): void {
    this.api.get().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected startEdit(profile: Profile): void {
    this.form.reset({
      firstName: profile.firstName,
      lastName: profile.lastName,
      username: profile.username,
      bio: profile.bio ?? '',
    });
    this.editError.set(null);
    this.avatarError.set(null);
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected save(): void {
    if (this.form.pending) {
      this.form.statusChanges
        .pipe(
          filter((status) => status !== 'PENDING'),
          take(1),
        )
        .subscribe(() => this.save());
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.editError.set(null);

    this.api
      .update({ firstName: value.firstName, lastName: value.lastName, username: value.username, bio: value.bio || null })
      .subscribe({
        next: (profile) => {
          this.applyProfile(profile);
          this.editing.set(false);
          this.saving.set(false);
        },
        error: (err) => {
          this.editError.set(extractErrorMessage(err));
          this.saving.set(false);
        },
      });
  }

  protected async onAvatarSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    this.avatarBusy.set(true);
    this.avatarError.set(null);

    let image: Blob;
    try {
      image = await cropToAvatar(file);
    } catch {
      this.avatarError.set(this.transloco.translate('profile.avatarReadError'));
      this.avatarBusy.set(false);
      return;
    }

    this.api.setAvatar(image).subscribe({
      next: (profile) => {
        this.applyProfile(profile);
        this.avatarBusy.set(false);
      },
      error: (err) => {
        this.avatarError.set(extractErrorMessage(err));
        this.avatarBusy.set(false);
      },
    });
  }

  protected removeAvatar(): void {
    this.avatarBusy.set(true);
    this.avatarError.set(null);

    this.api.removeAvatar().subscribe({
      next: (profile) => {
        this.applyProfile(profile);
        this.avatarBusy.set(false);
      },
      error: (err) => {
        this.avatarError.set(extractErrorMessage(err));
        this.avatarBusy.set(false);
      },
    });
  }

  private applyProfile(profile: Profile): void {
    this.profile.set(profile);
    this.auth.updateUser({
      displayName: profile.displayName,
      username: profile.username,
      avatarVersion: profile.avatarVersion,
    });
  }

  protected permissionKey(work: ProfileSharedWork): string {
    return work.permission === 'CanEdit' ? 'sharing.canEdit' : 'sharing.viewOnly';
  }

  protected openWork(work: ProfileSharedWork): void {
    const routes = { Pattern: 'matrix', Video: 'video', Site: 'site' };
    this.router.navigate(['/works', work.workId, routes[work.type]]);
  }

  protected onThemeChange(event: Event): void {
    this.theme.setMode((event.target as HTMLSelectElement).value as ThemeMode);
  }

  protected onLanguageChange(event: Event): void {
    this.language.setLanguage((event.target as HTMLSelectElement).value);
  }

  protected resetPassword(profile: Profile): void {
    const title = this.transloco.translate('profile.resetTitle');
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('profile.resetHint', { email: profile.email }),
      confirmLabel: this.transloco.translate('profile.resetConfirm'),
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
      width: '440px',
      modal: true,
      data,
    });

    ref?.onClose.subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.resetBusy.set(true);
      this.resetError.set(null);
      this.auth.startPasswordReset(profile.email).subscribe({
        error: (err) => {
          this.resetError.set(extractErrorMessage(err));
          this.resetBusy.set(false);
        },
      });
    });
  }
}
