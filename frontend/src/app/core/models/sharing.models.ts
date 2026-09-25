import { WorkType } from './work.models';

export type WorkPermission = 'ViewOnly' | 'CanEdit';

export interface Invitation {
  code: string;
  permission: WorkPermission;
  isActive: boolean;
}

export interface WorkMember {
  userId: string;
  displayName: string;
  username: string;
  avatarVersion: number | null;
  permission: WorkPermission;
  joinedAt: string;
}

export interface SharingInfo {
  invitation: Invitation | null;
  members: WorkMember[];
}

export interface JoinWorkResponse {
  workId: string;
  name: string;
  type: WorkType;
  permission: WorkPermission;
}
