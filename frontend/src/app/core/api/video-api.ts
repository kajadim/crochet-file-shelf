import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  UpdateVideoLinkRequest,
  UpdateVideoTimestampRequest,
  Video,
  VideoStatus,
} from '../models/video.models';

@Injectable({
  providedIn: 'root',
})
export class VideoApi {
  private readonly http = inject(HttpClient);

  private baseUrl(workId: string): string {
    return `/api/works/${workId}/video`;
  }

  get(workId: string): Observable<Video> {
    return this.http.get<Video>(this.baseUrl(workId));
  }

  getStatus(workId: string): Observable<VideoStatus> {
    return this.http.get<VideoStatus>(`${this.baseUrl(workId)}/status`);
  }

  updateLink(workId: string, request: UpdateVideoLinkRequest): Observable<Video> {
    return this.http.put<Video>(this.baseUrl(workId), request);
  }

  updateTimestamp(workId: string, request: UpdateVideoTimestampRequest): Observable<Video> {
    return this.http.put<Video>(`${this.baseUrl(workId)}/timestamp`, request);
  }
}
