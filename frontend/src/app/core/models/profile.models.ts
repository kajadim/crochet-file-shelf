import { WorkPermission } from './sharing.models';
import { WorkType } from './work.models';

export interface WorkCounts {
  pattern: number;
  video: number;
  site: number;
}

export interface ProfileSharedWork {
  workId: string;
  name: string;
  type: WorkType;
  permission: WorkPermission;
}

export interface ProfileCollaborator {
  userId: string;
  displayName: string;
  username: string;
  avatarVersion: number | null;
  works: ProfileSharedWork[];
}

export interface Profile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  username: string;
  bio: string | null;
  avatarVersion: number | null;
  createdAt: string;
  workCounts: WorkCounts;
  sharedByMe: ProfileCollaborator[];
  sharedWithMe: ProfileCollaborator[];
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  username: string;
  bio: string | null;
}
