import { Component, input, output } from '@angular/core';
import { ActionMenu, ActionMenuItem } from '../action-menu/action-menu';
import { Folder, FolderRow } from '../../core/models/folder.models';

@Component({
  selector: 'app-folder-tree',
  imports: [ActionMenu],
  templateUrl: './folder-tree.html',
  styleUrl: './folder-tree.scss',
})
export class FolderTree {
  readonly rows = input.required<FolderRow[]>();
  readonly selectedId = input<string | null>(null);

  readonly selectFolder = output<string>();
  readonly toggleFolder = output<string>();
  readonly createSubfolder = output<Folder>();
  readonly renameFolder = output<Folder>();
  readonly deleteFolder = output<Folder>();

  protected menuItems(folder: Folder): ActionMenuItem[] {
    return [
      { label: 'New subfolder', icon: 'pi pi-folder-plus', action: () => this.createSubfolder.emit(folder) },
      { label: 'Rename', icon: 'pi pi-pencil', action: () => this.renameFolder.emit(folder) },
      { label: 'Delete', icon: 'pi pi-trash', action: () => this.deleteFolder.emit(folder), danger: true },
    ];
  }
}
