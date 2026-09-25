import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Profile, UpdateProfileRequest } from '../models/profile.models';

@Injectable({
  providedIn: 'root',
})
export class ProfileApi {
  private readonly http = inject(HttpClient);

  get(): Observable<Profile> {
    return this.http.get<Profile>('/api/profile');
  }

  update(request: UpdateProfileRequest): Observable<Profile> {
    return this.http.put<Profile>('/api/profile', request);
  }

  setAvatar(image: Blob): Observable<Profile> {
    const body = new FormData();
    body.append('file', image, 'avatar.jpg');
    return this.http.put<Profile>('/api/profile/avatar', body);
  }

  removeAvatar(): Observable<Profile> {
    return this.http.delete<Profile>('/api/profile/avatar');
  }
}
