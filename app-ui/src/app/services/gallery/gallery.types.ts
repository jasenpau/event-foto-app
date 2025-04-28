export interface GalleryDataDto {
  name: string;
  watermarkId: number | null;
}

export interface GalleryDto {
  id: number;
  name: string;
  eventId: number;
  isMainGallery?: boolean;
  thumbnail: string | null;
  photoCount: number;
  watermarkId: number | null;
}
