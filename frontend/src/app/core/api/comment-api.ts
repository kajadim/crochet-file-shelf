import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CommentRequest, WorkComment } from '../models/comment.models';

@Injectable({
  providedIn: 'root',
})
export class CommentApi {
  private readonly http = inject(HttpClient);

  private baseUrl(workId: string): string {
    return `/api/works/${workId}/comments`;
  }

  getAll(workId: string): Observable<WorkComment[]> {
    return this.http.get<WorkComment[]>(this.baseUrl(workId));
  }

  create(workId: string, request: CommentRequest): Observable<WorkComment> {
    return this.http.post<WorkComment>(this.baseUrl(workId), request);
  }

  update(workId: string, commentId: string, request: CommentRequest): Observable<WorkComment> {
    return this.http.put<WorkComment>(`${this.baseUrl(workId)}/${commentId}`, request);
  }

  delete(workId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(workId)}/${commentId}`);
  }
}
