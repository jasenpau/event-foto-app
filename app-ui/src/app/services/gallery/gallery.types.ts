export interface PhotoListDto {
  id: number;
  isProcessed: boolean;
  processedFilename?: string;
  captureDate: string;
}

export interface PhotoSearchParamsDto {
  keyOffset?: string | null;
  pageSize?: number;
  eventId: number;
  fromDate?: Date;
  toDate?: Date;
}
