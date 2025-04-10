export interface EventListDto {
  id: number;
  name: string;
  isArchived: boolean;
  startDate: string;
  endDate?: string;
}

export interface EventCreateDto {
  name: string;
  startDate: Date;
  endDate?: Date;
  location?: string;
  note?: string;
}

export interface EventDto {
  id: number;
  name: string;
  startDate: string;
  endDate?: string;
  location?: string;
  note?: string;
  isArchived: boolean;
  createdAt: Date;
  createdByUser: string;
  createdBy: string;
}

export interface EventPhotographer {
  id: string;
  name: string;
  photoCount: number;
}

export interface EventSearchParamsDto {
  searchTerm?: string | null;
  keyOffset?: string | null;
  pageSize?: number;
  fromDate?: Date;
  toDate?: Date;
}
