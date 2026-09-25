import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { SharingApi } from '../../core/api/sharing-api';
import { SharingInfo, WorkMember, WorkPermission } from '../../core/models/sharing.models';
import { Work } from '../../core/models/work.models';
import { extractErrorMessage } from '../../core/utils/http-error';
import { ConfirmDialog, ConfirmDialogData } from '../confirm-dialog/confirm-dialog';

export interface ShareDialogData {
  work: Work;
}

@Component({
  selector: 'app-share-dialog',
  imports: [FormsModule, ButtonModule, TranslocoPipe],
  templateUrl: './share-dialog.html',
  styleUrl: './share-dialog.scss',
})
export class ShareDialog implements OnInit {
  private readonly api = inject(SharingApi);
  private readonly dialogService = inject(DialogService);
  private readonly transloco = inject(TranslocoService);
  protected readonly data = inject(DynamicDialogConfig).data as ShareDialogData;

  protected readonly info = signal<SharingInfo | null>(null);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly copied = signal(false);
  protected readonly newPermission = signal<WorkPermission>('CanEdit');

  ngOnInit(): void {
    this.api.get(this.data.work.id).subscribe({
      next: (info) => {
        this.info.set(info);
        if (info.invitation) {
          this.newPermission.set(info.invitation.permission);
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected createCode(): void {
    this.run(this.api.setInvitation(this.data.work.id, this.newPermission()), (invitation) =>
      this.info.update((info) => (info ? { ...info, invitation } : info)),
    );
  }

  protected revokeCode(): void {
    this.run(this.api.revokeInvitation(this.data.work.id), () =>
      this.info.update((info) =>
        info && info.invitation ? { ...info, invitation: { ...info.invitation, isActive: false } } : info,
      ),
    );
  }

  protected async copyCode(code: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(code);
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      this.copied.set(false);
    }
  }

  protected changePermission(member: WorkMember, permission: WorkPermission): void {
    this.run(this.api.updateMember(this.data.work.id, member.userId, permission), (updated) =>
      this.info.update((info) =>
        info ? { ...info, members: info.members.map((m) => (m.userId === updated.userId ? updated : m)) } : info,
      ),
    );
  }

  protected removeMember(member: WorkMember): void {
    const title = this.transloco.translate('sharing.removeTitle', { name: member.displayName });
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('sharing.removeMessage'),
      confirmLabel: this.transloco.translate('sharing.remove'),
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
      width: '420px',
      modal: true,
      data,
    });

    ref?.onClose.subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.run(this.api.removeMember(this.data.work.id, member.userId), () =>
        this.info.update((info) =>
          info ? { ...info, members: info.members.filter((m) => m.userId !== member.userId) } : info,
        ),
      );
    });
  }

  protected permissionKey(permission: WorkPermission): string {
    return permission === 'CanEdit' ? 'sharing.canEdit' : 'sharing.viewOnly';
  }

  private run<T>(request: Observable<T>, onSuccess: (result: T) => void): void {
    this.busy.set(true);
    this.error.set(null);

    request.subscribe({
      next: (result) => {
        onSuccess(result);
        this.busy.set(false);
      },
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.busy.set(false);
      },
    });
  }
}
