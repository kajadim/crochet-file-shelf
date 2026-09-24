export interface WorkComment {
  id: string;
  text: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CommentRequest {
  text: string;
}
