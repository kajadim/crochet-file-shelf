import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreatePatternRequest,
  ImportPreview,
  PatternEdges,
  Pattern,
  SetCellsRequest,
  UpdateActiveRowRequest,
  UpdatePositionRequest,
} from '../models/pattern.models';

@Injectable({
  providedIn: 'root',
})
export class PatternApi {
  private readonly http = inject(HttpClient);

  private baseUrl(workId: string): string {
    return `/api/works/${workId}/pattern`;
  }

  get(workId: string): Observable<Pattern> {
    return this.http.get<Pattern>(this.baseUrl(workId));
  }

  create(workId: string, request: CreatePatternRequest): Observable<Pattern> {
    return this.http.post<Pattern>(this.baseUrl(workId), request);
  }

  updatePosition(workId: string, request: UpdatePositionRequest): Observable<Pattern> {
    return this.http.put<Pattern>(`${this.baseUrl(workId)}/position`, request);
  }

  setCells(workId: string, request: SetCellsRequest): Observable<Pattern> {
    return this.http.put<Pattern>(`${this.baseUrl(workId)}/cells`, request);
  }

  expand(workId: string, request: PatternEdges): Observable<Pattern> {
    return this.http.put<Pattern>(`${this.baseUrl(workId)}/expand`, request);
  }

  shrink(workId: string, request: PatternEdges): Observable<Pattern> {
    return this.http.put<Pattern>(`${this.baseUrl(workId)}/shrink`, request);
  }

  exportFile(workId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl(workId)}/export`, { responseType: 'blob' });
  }

  previewImport(workId: string, file: File): Observable<ImportPreview> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ImportPreview>(`${this.baseUrl(workId)}/import/preview`, form);
  }

  previewImportFile(file: File): Observable<ImportPreview> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ImportPreview>('/api/patterns/import/preview', form);
  }

  importFile(workId: string, file: File): Observable<Pattern> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<Pattern>(`${this.baseUrl(workId)}/import`, form);
  }

  updateActiveRow(workId: string, request: UpdateActiveRowRequest): Observable<Pattern> {
    return this.http.put<Pattern>(`${this.baseUrl(workId)}/active-row`, request);
  }
}
