import { Injectable } from '@angular/core';

interface YouTubePlayerEvent {
  target: { getDuration(): number };
}

interface YouTubeApi {
  Player: new (
    element: HTMLIFrameElement,
    options: {
      events: {
        onReady: (event: YouTubePlayerEvent) => void;
        onStateChange: (event: YouTubePlayerEvent) => void;
        onError: () => void;
      };
    },
  ) => unknown;
}

declare global {
  interface Window {
    YT?: YouTubeApi;
    onYouTubeIframeAPIReady?: () => void;
  }
}

@Injectable({
  providedIn: 'root',
})
export class YouTubePlayerService {
  private apiPromise: Promise<void> | null = null;

  async getDuration(iframe: HTMLIFrameElement): Promise<number | null> {
    try {
      await this.loadApi();
    } catch {
      return null;
    }

    return new Promise((resolve) => {
      let finished = false;
      const finish = (value: number | null) => {
        if (!finished) {
          finished = true;
          clearInterval(poll);
          clearTimeout(timeout);
          resolve(value);
        }
      };

      let player: { getDuration?: () => number } | null = null;
      const readDuration = (source?: { getDuration(): number }) => {
        const duration = (source ?? player)?.getDuration?.() ?? 0;
        if (duration > 0) {
          finish(Math.floor(duration));
        }
      };

      const poll = setInterval(() => readDuration(), 500);
      const timeout = setTimeout(() => finish(null), 8000);

      player = new window.YT!.Player(iframe, {
        events: {
          onReady: (event) => readDuration(event.target),
          onStateChange: (event) => readDuration(event.target),
          onError: () => finish(null),
        },
      }) as { getDuration?: () => number };
    });
  }

  private loadApi(): Promise<void> {
    if (window.YT?.Player) {
      return Promise.resolve();
    }

    if (!this.apiPromise) {
      this.apiPromise = new Promise<void>((resolve, reject) => {
        const previous = window.onYouTubeIframeAPIReady;
        window.onYouTubeIframeAPIReady = () => {
          previous?.();
          resolve();
        };

        const script = document.createElement('script');
        script.src = 'https://www.youtube.com/iframe_api';
        script.onerror = () => {
          this.apiPromise = null;
          reject(new Error('YouTube API failed to load'));
        };
        document.head.appendChild(script);
      });
    }

    return this.apiPromise;
  }
}
