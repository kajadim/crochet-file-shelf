export type NotificationType =
  | 'WorkInvite'
  | 'RemovedFromWork'
  | 'MemberJoined'
  | 'WorkDeleted'
  | 'OwnershipTransferred';

export interface AppNotification {
  id: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: string;
  workId: string | null;
}

export interface NotificationList {
  items: AppNotification[];
  unreadCount: number;
}
