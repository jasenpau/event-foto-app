import { Injectable } from '@angular/core';
import { SasUri, SasUriResponse } from '../image/image.types';
import { HttpClient } from '@angular/common/http';
import { EnvService } from '../environment/env.service';
import { map, of, switchMap } from 'rxjs';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';

@Injectable({
  providedIn: 'root',
})
export class BlobService {
  private readonly apiBaseUrl;
  private readOnlySasUri?: SasUri;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
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
          if (parts[0].endsWith('/')) {
            parts[0] = parts[0].slice(0, -1);
          }

          this.readOnlySasUri = {
            baseUri: parts[0],
            params: parts[1],
            expiresOn: new Date(sasUriResponse.expiresOn),
          };
          return this.readOnlySasUri;
        }),
      );
  }

  getFromBlob(container: string, filename: string) {
    return this.getReadOnlySasUri().pipe(
      switchMap((sasUri) => {
        const sasUrl = `${sasUri.baseUri}/${container}/${filename}?${sasUri.params}`;
        return this.http.get(sasUrl, { responseType: 'blob' });
      }),
    );
  }
}
