export interface YarnColor {
  id: string;
  name: string;
  hexValue: string;
  notes: string | null;
  createdAt: string;
  worksUsingCount: number;
}

export interface YarnColorRequest {
  name: string;
  hexValue: string;
  notes: string | null;
}
