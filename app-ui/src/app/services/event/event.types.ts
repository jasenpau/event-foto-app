export interface EventData {
  id: number;
  name: string;
  isArchived: boolean;
  createdAt: Date;
}

export interface EventCreateDto {
  name: string;
}

export interface EventDto {
  id: number;
  name: string;
  isArchived: boolean;
  createdAt: Date;
}
