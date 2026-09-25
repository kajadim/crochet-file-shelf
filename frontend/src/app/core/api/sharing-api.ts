import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Invitation, JoinWorkResponse, SharingInfo, WorkMember, WorkPermission } from '../models/sharing.models';

@Injectable({
  providedIn: 'root',
})
export class SharingApi {
  private readonly http = inject(HttpClient);

  private baseUrl(workId: string): string {
    return `/api/works/${workId}/sharing`;
  }

  get(workId: string): Observable<SharingInfo> {
    return this.http.get<SharingInfo>(this.baseUrl(workId));
  }

  setInvitation(workId: string, permission: WorkPermission): Observable<Invitation> {
    return this.http.put<Invitation>(`${this.baseUrl(workId)}/invitation`, { permission });
  }

  revokeInvitation(workId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(workId)}/invitation`);
  }

  updateMember(workId: string, userId: string, permission: WorkPermission): Observable<WorkMember> {
    return this.http.put<WorkMember>(`${this.baseUrl(workId)}/members/${userId}`, { permission });
  }

  removeMember(workId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(workId)}/members/${userId}`);
  }

  leave(workId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(workId)}/membership`);
  }

  join(code: string): Observable<JoinWorkResponse> {
    return this.http.post<JoinWorkResponse>('/api/works/join', { code });
  }
}
