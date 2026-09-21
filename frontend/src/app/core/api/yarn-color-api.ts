import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { YarnColor, YarnColorRequest } from '../models/yarn-color.models';

@Injectable({
  providedIn: 'root',
})
export class YarnColorApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/yarn-colors';

  getAll(): Observable<YarnColor[]> {
    return this.http.get<YarnColor[]>(this.baseUrl);
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
