import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { ActionMenu, ActionMenuItem } from '../action-menu/action-menu';
import { Work } from '../../core/models/work.models';

@Component({
  selector: 'app-work-card',
  imports: [DatePipe, ActionMenu],
  templateUrl: './work-card.html',
  styleUrl: './work-card.scss',
})
export class WorkCard {
  readonly work = input.required<Work>();

  readonly edit = output<Work>();
  readonly move = output<Work>();
  readonly remove = output<Work>();

  protected readonly typeLabel = computed(() => (this.work().type === 'Pattern' ? 'Matrix' : 'Video'));
  protected readonly typeIcon = computed(() => (this.work().type === 'Pattern' ? 'pi-table' : 'pi-play-circle'));

  protected readonly menuItems = computed<ActionMenuItem[]>(() => [
    { label: 'Edit', icon: 'pi pi-pencil', action: () => this.edit.emit(this.work()) },
    { label: 'Move to...', icon: 'pi pi-arrow-right-arrow-left', action: () => this.move.emit(this.work()) },
    { label: 'Delete', icon: 'pi pi-trash', action: () => this.remove.emit(this.work()), danger: true },
  ]);
}
