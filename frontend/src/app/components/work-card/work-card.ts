import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Work } from '../../core/models/work.models';

@Component({
  selector: 'app-work-card',
  imports: [DatePipe, MatIconModule, MatMenuModule],
  templateUrl: './work-card.html',
  styleUrl: './work-card.scss',
})
export class WorkCard {
  readonly work = input.required<Work>();

  readonly edit = output<Work>();
  readonly move = output<Work>();
  readonly remove = output<Work>();

  protected readonly typeLabel = computed(() => (this.work().type === 'Pattern' ? 'Matrix' : 'Video'));
  protected readonly typeIcon = computed(() => (this.work().type === 'Pattern' ? 'grid_on' : 'play_circle'));
}
