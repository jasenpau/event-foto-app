import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvService } from '../environment/env.service';
import { GalleryDto } from './gallery.types';
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
}
