import { WorkType } from '../models/work.models';

const STORAGE_KEY = 'crochet.dashboardFilters';

export const PLATFORM_OPTIONS = ['YouTube', 'TikTok', 'Instagram', 'Pinterest'];
const WORK_TYPES: WorkType[] = ['Pattern', 'Video', 'Site'];

export type SharingFilter = 'shared' | 'private';
const SHARING_FILTERS: SharingFilter[] = ['shared', 'private'];

export interface DashboardFilters {
  search: string;
  type: WorkType | null;
  colorId: string | null;
  platform: string | null;
  sharing: SharingFilter | null;
}

const EMPTY_FILTERS: DashboardFilters = { search: '', type: null, colorId: null, platform: null, sharing: null };

export function loadDashboardFilters(): DashboardFilters {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return EMPTY_FILTERS;
    }

    const parsed = JSON.parse(raw) as Partial<DashboardFilters>;
    return {
      search: typeof parsed.search === 'string' ? parsed.search.slice(0, 200) : '',
      type: WORK_TYPES.includes(parsed.type as WorkType) ? (parsed.type as WorkType) : null,
      colorId: typeof parsed.colorId === 'string' ? parsed.colorId : null,
      platform: PLATFORM_OPTIONS.includes(parsed.platform as string) ? (parsed.platform as string) : null,
      sharing: SHARING_FILTERS.includes(parsed.sharing as SharingFilter) ? (parsed.sharing as SharingFilter) : null,
    };
  } catch {
    return EMPTY_FILTERS;
  }
}

export function saveDashboardFilters(filters: DashboardFilters): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(filters));
  } catch {
    return;
  }
}

export function clearDashboardFilters(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    return;
  }
}
