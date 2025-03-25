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
  isArchived: boolean;
  createdAt: Date;
}
