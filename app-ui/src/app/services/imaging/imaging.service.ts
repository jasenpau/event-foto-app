import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvService } from '../environment/env.service';

@Injectable({
  providedIn: 'root',
})
export class ImagingService {
  private readonly apiBaseUrl;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

  uploadImage(blob: Blob) {
    const formData = new FormData();
    formData.append('image', blob);

    return this.http.post(`${this.apiBaseUrl}/imaging/camera-upload`, formData);
  }
}
