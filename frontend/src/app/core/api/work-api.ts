import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateWorkRequest, MoveWorkRequest, UpdateWorkRequest, Work } from '../models/work.models';

@Injectable({
  providedIn: 'root',
})
export class WorkApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/works';

  getAll(folderId: string | null): Observable<Work[]> {
    const params = folderId ? new HttpParams().set('folderId', folderId) : undefined;
    return this.http.get<Work[]>(this.baseUrl, { params });
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
