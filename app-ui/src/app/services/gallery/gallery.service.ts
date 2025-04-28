import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvService } from '../environment/env.service';
import { GalleryDataDto, GalleryDto } from './gallery.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';

@Injectable({
  providedIn: 'root',
})
export class GalleryService {
  private readonly apiBaseUrl;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

  getGalleryDetails(id: number) {
    return this.http.get<GalleryDto>(`${this.apiBaseUrl}/gallery/${id}`, {
      ...getAuthHeaders(),
    });
  }

  getEventGalleries(eventId: number) {
    return this.http.get<GalleryDto[]>(
      `${this.apiBaseUrl}/gallery/event/${eventId}`,
      {
        ...getAuthHeaders(),
      },
    );
  }

  createEventGallery(eventId: number, galleryData: GalleryDataDto) {
    return this.http.post<GalleryDto>(
      `${this.apiBaseUrl}/gallery/event/${eventId}`,
      galleryData,
      {
        ...getAuthHeaders(),
      },
    );
  }

  updateGallery(galleryId: number, galleryData: GalleryDataDto) {
    return this.http.put<GalleryDto>(
      `${this.apiBaseUrl}/gallery/${galleryId}`,
      galleryData,
      { ...getAuthHeaders() },
    );
  }

  deleteGallery(galleryId: number) {
    return this.http.delete(`${this.apiBaseUrl}/gallery/${galleryId}`, {
      ...getAuthHeaders(),
    });
  }
}
