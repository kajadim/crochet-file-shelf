import { Injectable, computed, effect, signal } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';

const STORAGE_KEY = 'theme';
const MODES: ThemeMode[] = ['system', 'light', 'dark'];

function readStoredMode(): ThemeMode {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && MODES.includes(stored as ThemeMode)) {
      return stored as ThemeMode;
    }
  } catch {
    return 'system';
  }
  return 'system';
}

@Injectable({ providedIn: 'root' })
export class Theme {
  readonly options = MODES;

  private readonly modeState = signal<ThemeMode>(readStoredMode());
  private readonly systemDark = signal(false);

  readonly mode = this.modeState.asReadonly();
  readonly effective = computed(() => {
    const mode = this.modeState();
    if (mode === 'system') {
      return this.systemDark() ? 'dark' : 'light';
    }
    return mode;
  });

  constructor() {
    if (typeof window !== 'undefined' && window.matchMedia) {
      const query = window.matchMedia('(prefers-color-scheme: dark)');
      this.systemDark.set(query.matches);
      query.addEventListener('change', (event) => this.systemDark.set(event.matches));
    }

    effect(() => {
      document.documentElement.dataset['theme'] = this.effective();
    });
  }

  setMode(mode: ThemeMode): void {
    if (!MODES.includes(mode)) {
      return;
    }

    this.modeState.set(mode);

    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      return;
    }
  }
}
