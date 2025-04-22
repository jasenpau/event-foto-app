export interface PhotoListDto {
  id: number;
  isProcessed: boolean;
  processedFilename?: string;
  captureDate: string;
  selected?: boolean;
}

export interface PhotoDetailDto {
  id: number;
  filename: string;
  uploadDate: string;
  captureDate: string;
  isProcessed: boolean;
  processedFilename?: string;
  eventId: number;
  eventName: string;
  userId: string;
  userName: string;
}

export interface PhotoSearchParamsDto {
  keyOffset?: string | null;
  pageSize?: number;
  eventId: number;
  fromDate?: Date;
  toDate?: Date;
}

export interface OpenPhotoData {
  photo: PhotoListDto;
  eventId: number;
}

export enum PhotoAction {
  Close = 'close',
  Delete = 'delete',
}

export interface SasUriResponse {
  sasUri: string;
  expiresOn: string;
  eventId: number;
}

export interface ReadOnlySasUri {
  baseUri: string;
  params: string;
  expiresOn: Date;
}

export interface DownloadRequestDto {
  id: number;
  filename: string;
  userId: string;
  createdOn: string;
  isReady: boolean;
}
