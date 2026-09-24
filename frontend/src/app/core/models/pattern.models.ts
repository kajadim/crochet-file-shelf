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

export interface ImportPreviewColor {
  hex: string;
  existingName: string | null;
  count: number;
}

export interface ImportPreviewWarning {
  cell: string;
  reason: string;
}

export interface ImportPreview {
  width: number;
  height: number;
  coloredCells: number;
  skippedCells: number;
  colors: ImportPreviewColor[];
  warnings: ImportPreviewWarning[];
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
