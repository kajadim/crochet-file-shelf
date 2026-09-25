import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ActionMenu, ActionMenuItem } from '../action-menu/action-menu';
import { Work } from '../../core/models/work.models';

@Component({
  selector: 'app-work-card',
  imports: [DatePipe, ActionMenu, TranslocoPipe],
  templateUrl: './work-card.html',
  styleUrl: './work-card.scss',
})
export class WorkCard {
  readonly work = input.required<Work>();
  readonly folderPath = input<string | null>(null);

  readonly open = output<Work>();
  readonly edit = output<Work>();
  readonly move = output<Work>();
  readonly remove = output<Work>();
  readonly share = output<Work>();
  readonly leave = output<Work>();

  private static readonly typeKeys = { Pattern: 'workCard.matrix', Video: 'workCard.video', Site: 'workCard.site' };
  private static readonly typeIcons = { Pattern: 'pi-table', Video: 'pi-play-circle', Site: 'pi-globe' };

  protected readonly typeKey = computed(() => WorkCard.typeKeys[this.work().type]);
  protected readonly typeIcon = computed(() => WorkCard.typeIcons[this.work().type]);
  protected onCardClick(): void {
    this.open.emit(this.work());
  }

  protected readonly workActionsLabel = 'workCard.workActions';

  protected readonly isOwner = computed(() => this.work().role === 'Owner');
  protected readonly roleKey = computed(() => (this.work().role === 'Editor' ? 'sharing.canEdit' : 'sharing.viewOnly'));

  protected readonly menuItems = computed<ActionMenuItem[]>(() =>
    this.isOwner()
      ? [
          { label: 'common.edit', icon: 'pi pi-pencil', action: () => this.edit.emit(this.work()) },
          { label: 'workCard.moveTo', icon: 'pi pi-arrow-right-arrow-left', action: () => this.move.emit(this.work()) },
          { label: 'workCard.share', icon: 'pi pi-share-alt', action: () => this.share.emit(this.work()) },
          { label: 'common.delete', icon: 'pi pi-trash', action: () => this.remove.emit(this.work()), danger: true },
        ]
      : [{ label: 'workCard.leave', icon: 'pi pi-sign-out', action: () => this.leave.emit(this.work()), danger: true }],
  );
}
