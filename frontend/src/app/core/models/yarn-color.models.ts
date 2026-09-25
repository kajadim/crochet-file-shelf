export interface YarnColor {
  id: string;
  name: string;
  hexValue: string;
  notes: string | null;
  createdAt: string;
  worksUsingCount: number;
}

export type YarnColorSort = 'NameAsc' | 'NameDesc' | 'HexAsc' | 'HexDesc';

export interface YarnColorQuery {
  search?: string;
  sort?: YarnColorSort;
}

export interface YarnColorWork {
  id: string;
  name: string;
  type: 'Pattern' | 'Video' | 'Site';
}

export interface YarnColorRequest {
  name: string;
  hexValue: string;
  notes: string | null;
}
