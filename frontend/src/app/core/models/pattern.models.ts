export interface PatternCell {
  row: number;
  column: number;
  colorId: string;
  hexValue: string;
}

export interface Pattern {
  width: number;
  height: number;
  currentRow: number;
  currentColumn: number;
  activeRow: number | null;
  cells: PatternCell[];
}

export interface CreatePatternRequest {
  width: number;
  height: number;
}

export interface UpdatePositionRequest {
  row: number;
  column: number;
}

export interface PatternEdges {
  top: number;
  bottom: number;
  left: number;
  right: number;
}

export interface UpdateActiveRowRequest {
  row: number | null;
}

export interface CellChange {
  row: number;
  column: number;
  colorId: string | null;
}

export interface SetCellsRequest {
  cells: CellChange[];
}
