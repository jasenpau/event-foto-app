import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { EnvService } from '../environment/env.service';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { WatermarkDto } from './watermark.types';

@Injectable({
  providedIn: 'root',
})
export class WatermarkService {
  private readonly apiBaseUrl;
  private readonly watermarksContainerSetting;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
    this.watermarksContainerSetting =
      this.envService.getConfig().watermarksContainer;
  }

  get watermarksContainer() {
    return this.watermarksContainerSetting;
  }

  searchWatermarks(
    searchTerm: string | null,
    keyOffset: number | null,
    pageSize?: number,
  ) {
    let params = new HttpParams();
    if (searchTerm) params = params.append('query', searchTerm);
    if (keyOffset) params = params.append('keyOffset', keyOffset.toString());
    if (pageSize) params = params.append('pageSize', pageSize.toString());

    return this.http.get<PagedData<number, WatermarkDto>>(
      `${this.apiBaseUrl}/watermark/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }

  uploadWatermark(name: string, file: File) {
    const formData = new FormData();
    formData.append('name', name);
    formData.append('file', file);

    console.log(file);

    return this.http.post<WatermarkDto>(
      `${this.apiBaseUrl}/watermark`,
      formData,
      {
        ...getAuthHeaders(),
      },
    );
  }

  getWatermark(id: number) {
    return this.http.get<WatermarkDto>(`${this.apiBaseUrl}/watermark/${id}`, {
      ...getAuthHeaders(),
    });
  }

  deleteWatermark(id: number) {
    return this.http.delete<void>(`${this.apiBaseUrl}/watermark/${id}`, {
      ...getAuthHeaders(),
    });
  }
}
