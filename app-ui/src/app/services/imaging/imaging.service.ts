import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiBaseUrl } from '../../globals/variables';

@Injectable({
  providedIn: 'root'
})
export class ImagingService {

  constructor(private readonly http: HttpClient) { }

  uploadImage(blob: Blob) {
    const formData = new FormData();
    formData.append('image', blob);

    return this.http.post(`${ApiBaseUrl}/imaging/camera-upload`, formData);
  }
}
