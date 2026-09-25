export type WorkType = 'Pattern' | 'Video' | 'Site';
export type WorkRole = 'Owner' | 'Editor' | 'Viewer';

export interface Work {
  id: string;
  name: string;
  description: string | null;
  type: WorkType;
  folderId: string;
  createdAt: string;
  updatedAt: string;
  role: WorkRole;
  ownerName: string | null;
  isShared: boolean;
}

export interface CreateWorkRequest {
  name: string;
  description: string | null;
  type: WorkType;
  folderId: string;
  url?: string | null;
  width?: number | null;
  height?: number | null;
}

export interface WorkQuery {
  folderId?: string | null;
  shared?: boolean;
  search?: string;
  type?: WorkType | null;
  colorId?: string | null;
  platform?: string | null;
  isShared?: boolean | null;
}

export interface UpdateWorkRequest {
  name: string;
  description: string | null;
}

export interface MoveWorkRequest {
  folderId: string;
}
