export interface GalleryDto {
  id: number;
  name: string;
  eventId: number;
  isMainGallery?: boolean;
  thumbnail: string | null;
  photoCount: number;
}
