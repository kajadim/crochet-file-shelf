export interface WorkComment {
  id: string;
  text: string;
  authorId: string | null;
  authorName: string;
  authorUsername: string;
  authorAvatarVersion: number | null;
  canEdit: boolean;
  canDelete: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CommentRequest {
  text: string;
}
