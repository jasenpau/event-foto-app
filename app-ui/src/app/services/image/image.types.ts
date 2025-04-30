export interface PhotoListDto {
  id: number;
  isProcessed: boolean;
  processedFilename?: string;
  captureDate: string;
  selected?: boolean;
  photographerId: string;
  photographerName: string;
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
  galleryId: number;
  fromDate?: Date;
  toDate?: Date;
}

export interface OpenPhotoData {
  photo: PhotoListDto;
  eventId: number;
  showNext: boolean;
  showPrevious: boolean;
}

export enum PhotoAction {
  Close = 'close',
  Delete = 'delete',
  Previous = 'previous',
  Next = 'next',
}

export interface SasUriResponse {
  sasUri: string;
  expiresOn: string;
  eventId?: number;
}

export interface SasUri {
  baseUri: string;
  params: string;
  expiresOn: Date;
  eventId?: number;
}

export interface DownloadRequestDto {
  id: number;
  filename: string;
  userId: string;
  createdOn: string;
  isReady: boolean;
}

export interface BatchResponse {
  id: number;
  photoCount: number;
  ready: boolean;
}
