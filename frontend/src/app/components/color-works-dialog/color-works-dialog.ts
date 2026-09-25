import { Component, inject } from '@angular/core';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ColorWorksList } from '../color-works-list/color-works-list';

export interface ColorWorksDialogData {
  colorId: string;
}

@Component({
  selector: 'app-color-works-dialog',
  imports: [ColorWorksList],
  template: `<app-color-works-list [colorId]="data.colorId" (opened)="dialogRef.close()" />`,
})
export class ColorWorksDialog {
  protected readonly dialogRef = inject(DynamicDialogRef);
  protected readonly data = inject(DynamicDialogConfig).data as ColorWorksDialogData;
}
