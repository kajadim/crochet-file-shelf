import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Folder, FolderRow } from '../../core/models/folder.models';

@Component({
  selector: 'app-folder-tree',
  imports: [MatIconModule, MatMenuModule],
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
}
