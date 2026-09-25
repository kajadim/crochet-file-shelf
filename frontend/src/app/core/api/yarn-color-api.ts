import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { YarnColor, YarnColorQuery, YarnColorRequest, YarnColorWork } from '../models/yarn-color.models';

@Injectable({
  providedIn: 'root',
})
export class YarnColorApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/yarn-colors';

  getAll(query: YarnColorQuery = {}): Observable<YarnColor[]> {
    let params = new HttpParams();
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.sort) {
      params = params.set('sort', query.sort);
    }
    return this.http.get<YarnColor[]>(this.baseUrl, { params });
  }

  getWorks(id: string): Observable<YarnColorWork[]> {
    return this.http.get<YarnColorWork[]>(`${this.baseUrl}/${id}/works`);
  }

  create(request: YarnColorRequest): Observable<YarnColor> {
    return this.http.post<YarnColor>(this.baseUrl, request);
  }

  update(id: string, request: YarnColorRequest): Observable<YarnColor> {
    return this.http.put<YarnColor>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
