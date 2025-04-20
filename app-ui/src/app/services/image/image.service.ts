import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PagedData } from '../../components/paged-table/paged-table.types';
import {
  BulkActionType,
  PhotoDetailDto,
  PhotoListDto,
  PhotoSearchParamsDto,
  ReadOnlySasUri,
  SasUriResponse,
} from './image.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { map, of } from 'rxjs';
import { EnvService } from '../environment/env.service';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  private readonly apiBaseUrl;
  private readOnlySasUri?: ReadOnlySasUri;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

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
      `${this.apiBaseUrl}/image/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }

  getPhotoDetails(photoId: number) {
    return this.http.get<PhotoDetailDto>(
      `${this.apiBaseUrl}/image/details/${photoId}`,
      getAuthHeaders(),
    );
  }

  getRawPhoto(eventId: number, filename: string) {
    return this.http.get(
      `${this.apiBaseUrl}/image/raw/${eventId}/${filename}`,
      {
        ...getAuthHeaders(),
        responseType: 'blob',
      },
    );
  }

  bulkAction(actionType: BulkActionType, photoIds: number[]) {
    return this.http
      .post<string>(
        `${this.apiBaseUrl}/image/bulk-action`,
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

  getReadOnlySasUri() {
    if (this.readOnlySasUri && this.readOnlySasUri.expiresOn > new Date()) {
      return of(this.readOnlySasUri);
    }

    return this.http
      .get<SasUriResponse>(`${this.apiBaseUrl}/image/sas`, {
        ...getAuthHeaders(),
      })
      .pipe(
        map((sasUriResponse) => {
          const parts = sasUriResponse.sasUri.split('?');
          this.readOnlySasUri = {
            baseUri: parts[0],
            params: parts[1],
            expiresOn: new Date(sasUriResponse.expiresOn),
          };
          return this.readOnlySasUri;
        }),
      );
  }
}
