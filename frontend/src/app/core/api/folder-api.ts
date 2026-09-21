import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateFolderRequest,
  Folder,
  FolderDeletionSummary,
  RenameFolderRequest,
} from '../models/folder.models';

@Injectable({
  providedIn: 'root',
})
export class FolderApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/folders';

  getAll(): Observable<Folder[]> {
    return this.http.get<Folder[]>(this.baseUrl);
  }

  create(request: CreateFolderRequest): Observable<Folder> {
    return this.http.post<Folder>(this.baseUrl, request);
  }

  rename(id: string, request: RenameFolderRequest): Observable<Folder> {
    return this.http.put<Folder>(`${this.baseUrl}/${id}`, request);
  }

  getDeletionSummary(id: string): Observable<FolderDeletionSummary> {
    return this.http.get<FolderDeletionSummary>(`${this.baseUrl}/${id}/deletion-summary`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
