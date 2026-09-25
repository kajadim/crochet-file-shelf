export interface WorkComment {
  id: string;
  text: string;
  authorName: string;
  canEdit: boolean;
  canDelete: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CommentRequest {
  text: string;
}
