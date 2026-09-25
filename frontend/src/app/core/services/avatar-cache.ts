import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AvatarCache {
  private readonly http = inject(HttpClient);
  private readonly entries = new Map<string, Promise<string | null>>();

  load(userId: string, version: number): Promise<string | null> {
    const key = `${userId}:${version}`;
    let entry = this.entries.get(key);

    if (!entry) {
      entry = this.fetch(userId).catch(() => {
        this.entries.delete(key);
        return null;
      });
      this.entries.set(key, entry);
    }

    return entry;
  }

  private async fetch(userId: string): Promise<string | null> {
    const response = await firstValueFrom(
      this.http.get(`/api/users/${userId}/avatar`, { responseType: 'blob', observe: 'response' }),
    );
    return response.status === 200 && response.body ? URL.createObjectURL(response.body) : null;
  }
}
