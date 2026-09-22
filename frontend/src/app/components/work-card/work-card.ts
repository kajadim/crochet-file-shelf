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

  readonly edit = output<Work>();
  readonly move = output<Work>();
  readonly remove = output<Work>();

  protected readonly typeKey = computed(() => (this.work().type === 'Pattern' ? 'workCard.matrix' : 'workCard.video'));
  protected readonly typeIcon = computed(() => (this.work().type === 'Pattern' ? 'pi-table' : 'pi-play-circle'));

  protected readonly workActionsLabel = 'workCard.workActions';

  protected readonly menuItems = computed<ActionMenuItem[]>(() => [
    { label: 'common.edit', icon: 'pi pi-pencil', action: () => this.edit.emit(this.work()) },
    { label: 'workCard.moveTo', icon: 'pi pi-arrow-right-arrow-left', action: () => this.move.emit(this.work()) },
    { label: 'common.delete', icon: 'pi pi-trash', action: () => this.remove.emit(this.work()), danger: true },
  ]);
}
