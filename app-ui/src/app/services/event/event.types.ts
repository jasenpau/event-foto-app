export interface EventListDto {
  id: number;
  name: string;
  isArchived: boolean;
  startDate: string;
  endDate?: string;
  thumbnail?: string;
  photoCount: number;
}

export interface EventCreateDto {
  name: string;
  startDate: Date;
  endDate?: Date;
  location?: string;
  note?: string;
  watermarkId?: number | null;
}

export interface EventDto {
  id: number;
  name: string;
  startDate: string;
  endDate?: string;
  location?: string;
  note?: string;
  isArchived: boolean;
  archiveName?: string;
  createdAt: Date;
  createdByUser: string;
  createdBy: string;
  watermarkId?: number | null;
  defaultGalleryId: number;
}

export interface PhotographerAssignment {
  userId: string;
  userName: string;
  galleryId: number;
  galleryName: string;
}

export interface EventSearchParamsDto {
  searchTerm?: string | null;
  keyOffset?: string | null;
  pageSize?: number;
  fromDate?: Date;
  toDate?: Date;
  showArchived?: boolean;
}
