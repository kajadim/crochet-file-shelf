import { Component, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ActionMenu, ActionMenuItem } from '../../components/action-menu/action-menu';
import { ColorWorksDialog, ColorWorksDialogData } from '../../components/color-works-dialog/color-works-dialog';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { YarnColor, YarnColorSort } from '../../core/models/yarn-color.models';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-palette',
  imports: [FormsModule, ButtonModule, InputTextModule, SelectModule, ActionMenu, TranslocoPipe],
  templateUrl: './palette.html',
  styleUrl: './palette.scss',
})
export class Palette {
  protected readonly store = inject(YarnColorStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  protected readonly loadError = signal<string | null>(null);
  protected readonly colorActionsLabel = 'colorActions';

  protected readonly sortOptions: { value: YarnColorSort; label: string }[] = [
    { value: 'NameAsc', label: 'palette.sortNameAsc' },
    { value: 'NameDesc', label: 'palette.sortNameDesc' },
    { value: 'HexAsc', label: 'palette.sortHexAsc' },
    { value: 'HexDesc', label: 'palette.sortHexDesc' },
  ];

  protected readonly searchInput = signal('');
  protected readonly search = signal('');
  protected readonly sort = signal<YarnColorSort>('NameAsc');

  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  private loadSubscription: Subscription | null = null;

  constructor() {
    this.store.reset();

    effect(() => {
      const search = this.search();
      const sort = this.sort();

      untracked(() => {
        this.loadSubscription?.unsubscribe();
        this.loadError.set(null);
        this.loadSubscription = this.store.load({ search, sort }).subscribe({
          error: (error) => this.loadError.set(extractErrorMessage(error)),
        });
      });
    });

    inject(DestroyRef).onDestroy(() => {
      if (this.searchTimer) {
        clearTimeout(this.searchTimer);
      }
      this.loadSubscription?.unsubscribe();
    });
  }

  protected onSearchInput(value: string): void {
    this.searchInput.set(value);

    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchTimer = setTimeout(() => this.search.set(value.trim()), 300);
  }

  protected clearSearch(): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchInput.set('');
    this.search.set('');
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

  protected openWorks(color: YarnColor): void {
    this.dialogService.open<ColorWorksDialog, ColorWorksDialogData>(ColorWorksDialog, {
      header: this.transloco.translate('palette.usedInTitle', { name: color.name }),
      width: '420px',
      modal: true,
      closable: true,
      closeOnEscape: true,
      dismissableMask: true,
      data: { colorId: color.id },
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
