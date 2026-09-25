import { WorkRole } from './work.models';

export interface CellChangeEvent {
  row: number;
  column: number;
  colorId: string | null;
  hexValue: string | null;
}

export interface CellsChangedEvent {
  workId: string;
  cells: CellChangeEvent[];
}

export interface PositionChangedEvent {
  workId: string;
  row: number;
  column: number;
}

export interface ActiveRowChangedEvent {
  workId: string;
  row: number | null;
}

export interface WorkEvent {
  workId: string;
}

export interface AccessChangedEvent {
  workId: string;
  role: WorkRole | null;
}

export interface PresenceUser {
  userId: string;
  displayName: string;
  username: string;
  avatarVersion: number | null;
}

export interface PresenceChangedEvent {
  workId: string;
  users: PresenceUser[];
}
