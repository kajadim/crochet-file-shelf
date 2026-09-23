export type WorkType = 'Pattern' | 'Video' | 'Site';

export interface Work {
  id: string;
  name: string;
  description: string | null;
  type: WorkType;
  folderId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateWorkRequest {
  name: string;
  description: string | null;
  type: WorkType;
  folderId: string;
  url?: string | null;
}

export interface UpdateWorkRequest {
  name: string;
  description: string | null;
}

export interface MoveWorkRequest {
  folderId: string;
}
