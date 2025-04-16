import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { PhotoListDto, PhotoSearchParamsDto } from './gallery.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { ApiBaseUrl } from '../../globals/variables';

@Injectable({
  providedIn: 'root',
})
export class GalleryService {
  constructor(private http: HttpClient) {}

  searchPhotos(searchParams: PhotoSearchParamsDto) {
    let params = new HttpParams().append(
      'eventId',
      searchParams.eventId.toString(),
    );

    if (searchParams.keyOffset)
      params = params.append('keyOffset', searchParams.keyOffset);
    if (searchParams.pageSize)
      params = params.append('pageSize', searchParams.pageSize);
    if (searchParams.fromDate)
      params = params.append('fromDate', searchParams.fromDate.toISOString());
    if (searchParams.toDate)
      params = params.append('toDate', searchParams.toDate.toISOString());

    return this.http.get<PagedData<string, PhotoListDto>>(
      `${ApiBaseUrl}/gallery/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }
}
