import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { NotificationList } from '../models/notification.models';

@Injectable({
  providedIn: 'root',
})
export class NotificationApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/notifications';

  getAll(): Observable<NotificationList> {
    return this.http.get<NotificationList>(this.baseUrl);
  }

  markRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/read`, null);
  }

  markAllRead(): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/read-all`, null);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
