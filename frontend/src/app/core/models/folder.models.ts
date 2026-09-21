export interface Folder {
  id: string;
  name: string;
  parentFolderId: string | null;
  createdAt: string;
}

export interface CreateFolderRequest {
  name: string;
  parentFolderId: string | null;
}

export interface RenameFolderRequest {
  name: string;
}

export interface FolderDeletionSummary {
  subfolderCount: number;
  workCount: number;
}

export interface FolderRow {
  folder: Folder;
  depth: number;
  hasChildren: boolean;
  expanded: boolean;
}
