import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { YarnColorApi } from '../api/yarn-color-api';
import { YarnColor, YarnColorRequest } from '../models/yarn-color.models';

@Injectable({
  providedIn: 'root',
})
export class YarnColorStore {
  private readonly api = inject(YarnColorApi);

  private readonly colorsState = signal<YarnColor[]>([]);
  private readonly loadedState = signal(false);

  readonly colors = this.colorsState.asReadonly();
  readonly loaded = this.loadedState.asReadonly();

  reset(): void {
    this.colorsState.set([]);
    this.loadedState.set(false);
  }

  load(): Observable<YarnColor[]> {
    return this.api.getAll().pipe(
      tap((colors) => {
        this.colorsState.set(this.sorted(colors));
        this.loadedState.set(true);
      }),
    );
  }

  create(request: YarnColorRequest): Observable<YarnColor> {
    return this.api
      .create(request)
      .pipe(tap((color) => this.colorsState.update((colors) => this.sorted([...colors, color]))));
  }

  update(id: string, request: YarnColorRequest): Observable<YarnColor> {
    return this.api
      .update(id, request)
      .pipe(
        tap((updated) =>
          this.colorsState.update((colors) =>
            this.sorted(colors.map((color) => (color.id === id ? updated : color))),
          ),
        ),
      );
  }

  remove(id: string): Observable<void> {
    return this.api
      .delete(id)
      .pipe(tap(() => this.colorsState.update((colors) => colors.filter((color) => color.id !== id))));
  }

  private sorted(colors: YarnColor[]): YarnColor[] {
    return [...colors].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
  }
}
