import { Component, ElementRef, HostListener, inject, input, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

export interface ActionMenuItem {
  label: string;
  icon: string;
  action: () => void;
  danger?: boolean;
}

@Component({
  selector: 'app-action-menu',
  imports: [TranslocoPipe],
  templateUrl: './action-menu.html',
  styleUrl: './action-menu.scss',
})
export class ActionMenu {
  readonly items = input.required<ActionMenuItem[]>();
  readonly ariaLabel = input('common.edit');

  private readonly elementRef = inject(ElementRef<HTMLElement>);

  protected readonly open = signal(false);
  protected readonly position = signal({ top: 0, left: 0 });

  protected toggle(event: MouseEvent): void {
    event.stopPropagation();

    if (this.open()) {
      this.open.set(false);
      return;
    }

    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const width = 192;
    this.position.set({
      top: rect.bottom + 4,
      left: Math.min(Math.max(8, rect.right - width), window.innerWidth - width - 8),
    });
    this.open.set(true);
    setTimeout(() => this.elementRef.nativeElement.querySelector('.action-menu__item')?.focus());
  }

  protected select(event: MouseEvent, item: ActionMenuItem): void {
    event.stopPropagation();
    this.open.set(false);
    item.action();
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.elementRef.nativeElement.contains(event.target as Node)) {
      this.open.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.open.set(false);
  }
}
