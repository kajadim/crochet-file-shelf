import { Injectable, inject, signal } from '@angular/core';
import { Observable, map, switchMap, tap } from 'rxjs';
import { YarnColorApi } from '../api/yarn-color-api';
import { YarnColor, YarnColorQuery, YarnColorRequest } from '../models/yarn-color.models';

@Injectable({
  providedIn: 'root',
})
export class YarnColorStore {
  private readonly api = inject(YarnColorApi);

  private readonly colorsState = signal<YarnColor[]>([]);
  private readonly loadedState = signal(false);
  private currentQuery: YarnColorQuery = {};

  readonly colors = this.colorsState.asReadonly();
  readonly loaded = this.loadedState.asReadonly();

  reset(): void {
    this.currentQuery = {};
    this.colorsState.set([]);
    this.loadedState.set(false);
  }

  load(query: YarnColorQuery = {}): Observable<YarnColor[]> {
    this.currentQuery = query;
    return this.api.getAll(query).pipe(
      tap((colors) => {
        this.colorsState.set(colors);
        this.loadedState.set(true);
      }),
    );
  }

  create(request: YarnColorRequest): Observable<YarnColor> {
    return this.api.create(request).pipe(
      switchMap((color) => this.refetch().pipe(map(() => color))),
    );
  }

  update(id: string, request: YarnColorRequest): Observable<YarnColor> {
    return this.api.update(id, request).pipe(
      switchMap((updated) => this.refetch().pipe(map(() => updated))),
    );
  }

  remove(id: string): Observable<void> {
    return this.api
      .delete(id)
      .pipe(tap(() => this.colorsState.update((colors) => colors.filter((color) => color.id !== id))));
  }

  private refetch(): Observable<YarnColor[]> {
    return this.load(this.currentQuery);
  }
}
