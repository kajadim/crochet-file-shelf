import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateWorkRequest, MoveWorkRequest, UpdateWorkRequest, Work, WorkQuery } from '../models/work.models';

@Injectable({
  providedIn: 'root',
})
export class WorkApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/works';

  getAll(query: WorkQuery): Observable<Work[]> {
    let params = new HttpParams();
    if (query.folderId && !query.shared) {
      params = params.set('folderId', query.folderId);
    }
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.type) {
      params = params.set('type', query.type);
    }
    if (query.colorId) {
      params = params.set('colorId', query.colorId);
    }
    if (query.platform) {
      params = params.set('platform', query.platform);
    }
    if (query.isShared !== undefined && query.isShared !== null && !query.shared) {
      params = params.set('isShared', query.isShared);
    }
    return this.http.get<Work[]>(query.shared ? `${this.baseUrl}/shared` : this.baseUrl, { params });
  }

  getById(id: string): Observable<Work> {
    return this.http.get<Work>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateWorkRequest): Observable<Work> {
    return this.http.post<Work>(this.baseUrl, request);
  }

  update(id: string, request: UpdateWorkRequest): Observable<Work> {
    return this.http.put<Work>(`${this.baseUrl}/${id}`, request);
  }

  move(id: string, request: MoveWorkRequest): Observable<Work> {
    return this.http.put<Work>(`${this.baseUrl}/${id}/move`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
