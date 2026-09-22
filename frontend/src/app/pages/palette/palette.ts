import { Component, OnInit, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ActionMenu, ActionMenuItem } from '../../components/action-menu/action-menu';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { YarnColor } from '../../core/models/yarn-color.models';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-palette',
  imports: [ButtonModule, ActionMenu, TranslocoPipe],
  templateUrl: './palette.html',
  styleUrl: './palette.scss',
})
export class Palette implements OnInit {
  protected readonly store = inject(YarnColorStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  protected readonly loadError = signal<string | null>(null);
  protected readonly colorActionsLabel = 'colorActions';

  ngOnInit(): void {
    this.store.reset();
    this.loadError.set(null);

    this.store.load().subscribe({
      error: (error) => this.loadError.set(extractErrorMessage(error)),
    });
  }

  protected openCreate(): void {
    this.dialogService.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      header: this.transloco.translate('palette.addColor'),
      width: '460px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.create'),
        color: null,
        submit: (request) => this.store.create(request),
      },
    });
  }

  protected openEdit(color: YarnColor): void {
    this.dialogService.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      header: this.transloco.translate('palette.editColor'),
      width: '460px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.save'),
        color,
        submit: (request) => this.store.update(color.id, request),
      },
    });
  }

  protected usedInLabel(color: YarnColor): string {
    const unit = this.transloco.translate(color.worksUsingCount === 1 ? 'common.work' : 'common.works');
    return this.transloco.translate('palette.usedIn', { count: color.worksUsingCount, unit });
  }

  protected menuItems(color: YarnColor): ActionMenuItem[] {
    return [
      { label: 'common.edit', icon: 'pi pi-pencil', action: () => this.openEdit(color) },
      { label: 'common.delete', icon: 'pi pi-trash', action: () => this.delete(color), danger: true },
    ];
  }

  private delete(color: YarnColor): void {
    const usage = color.worksUsingCount;
    const title = this.transloco.translate('palette.deleteTitle', { name: color.name });
    const data: ConfirmDialogData = {
      title,
      message:
        usage > 0
          ? this.transloco.translate('palette.deleteMessageInUse', {
              count: usage,
              unit: this.transloco.translate(usage === 1 ? 'common.work' : 'common.works'),
            })
          : this.transloco.translate('common.cannotBeUndone'),
      confirmLabel: this.transloco.translate('common.delete'),
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
      this.store.remove(color.id).subscribe({
        error: (error) =>
          this.messageService.add({
            severity: 'error',
            summary: this.transloco.translate('common.error'),
            detail: extractErrorMessage(error),
          }),
      });
    });
  }
}
