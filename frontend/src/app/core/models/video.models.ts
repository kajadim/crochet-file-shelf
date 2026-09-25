export type VideoPlatform = 'YouTube' | 'TikTok' | 'Instagram' | 'Pinterest';

export interface Video {
  platform: VideoPlatform;
  originalUrl: string;
  normalizedUrl: string;
  embedUrl: string | null;
  timestampSeconds: number | null;
}

export interface VideoStatus {
  available: boolean | null;
  embeddable: boolean | null;
}

export interface UpdateVideoLinkRequest {
  url: string;
}

export interface UpdateVideoTimestampRequest {
  seconds: number | null;
}
