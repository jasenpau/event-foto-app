import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PagedData } from '../../components/paged-table/paged-table.types';
import {
  BulkActionType,
  PhotoDetailDto,
  PhotoListDto,
  PhotoSearchParamsDto,
} from './image.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { ApiBaseUrl } from '../../globals/variables';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
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
      `${ApiBaseUrl}/image/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }

  getPhotoDetails(photoId: number) {
    return this.http.get<PhotoDetailDto>(
      `${ApiBaseUrl}/image/details/${photoId}`,
      getAuthHeaders(),
    );
  }

  getRawPhoto(eventId: number, filename: string) {
    return this.http.get(`${ApiBaseUrl}/image/raw/${eventId}/${filename}`, {
      ...getAuthHeaders(),
      responseType: 'blob',
    });
  }

  bulkAction(actionType: BulkActionType, photoIds: number[]) {
    return this.http
      .post<string>(
        `${ApiBaseUrl}/image/bulk-action`,
        {
          action: actionType,
          photoIds,
        },
        {
          ...getAuthHeaders(),
        },
      )
      .pipe(map((x) => Number(x)));
  }
}
