import { Component, OnInit, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { ActionMenu, ActionMenuItem } from '../../components/action-menu/action-menu';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { YarnColor } from '../../core/models/yarn-color.models';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-palette',
  imports: [ButtonModule, ActionMenu],
  templateUrl: './palette.html',
  styleUrl: './palette.scss',
})
export class Palette implements OnInit {
  protected readonly store = inject(YarnColorStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);

  protected readonly loadError = signal<string | null>(null);

  ngOnInit(): void {
    this.store.reset();
    this.loadError.set(null);

    this.store.load().subscribe({
      error: (error) => this.loadError.set(extractErrorMessage(error)),
    });
  }

  protected openCreate(): void {
    this.dialogService.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      header: 'Add color',
      width: '460px',
      modal: true,
      data: {
        submitLabel: 'Add',
        color: null,
        submit: (request) => this.store.create(request),
      },
    });
  }

  protected openEdit(color: YarnColor): void {
    this.dialogService.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      header: 'Edit color',
      width: '460px',
      modal: true,
      data: {
        submitLabel: 'Save',
        color,
        submit: (request) => this.store.update(color.id, request),
      },
    });
  }

  protected menuItems(color: YarnColor): ActionMenuItem[] {
    return [
      { label: 'Edit', icon: 'pi pi-pencil', action: () => this.openEdit(color) },
      { label: 'Delete', icon: 'pi pi-trash', action: () => this.delete(color), danger: true },
    ];
  }

  private delete(color: YarnColor): void {
    const usage = color.worksUsingCount;
    const data: ConfirmDialogData = {
      title: `Delete "${color.name}"?`,
      message:
        usage > 0
          ? `This color is used in ${usage} ${usage === 1 ? 'work' : 'works'}. It will be removed from your palette, but works that already use it will keep it.`
          : 'This cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: data.title,
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
          this.messageService.add({ severity: 'error', summary: 'Error', detail: extractErrorMessage(error) }),
      });
    });
  }
}
