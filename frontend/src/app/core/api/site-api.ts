import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Site, UpdateSiteRequest } from '../models/site.models';

@Injectable({
  providedIn: 'root',
})
export class SiteApi {
  private readonly http = inject(HttpClient);

  private baseUrl(workId: string): string {
    return `/api/works/${workId}/site`;
  }

  get(workId: string): Observable<Site> {
    return this.http.get<Site>(this.baseUrl(workId));
  }

  update(workId: string, request: UpdateSiteRequest): Observable<Site> {
    return this.http.put<Site>(this.baseUrl(workId), request);
  }
}
