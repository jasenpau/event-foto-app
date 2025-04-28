import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PagedData } from '../../components/paged-table/paged-table.types';
import {
  BatchResponse,
  DownloadRequestDto,
  PhotoDetailDto,
  PhotoListDto,
  PhotoSearchParamsDto,
  SasUri,
  SasUriResponse,
} from './image.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import {
  firstValueFrom,
  map,
  of,
  repeat,
  skipWhile,
  Subject,
  switchMap,
  take,
  tap,
} from 'rxjs';
import { EnvService } from '../environment/env.service';
import { SnackbarType } from '../snackbar/snackbar.types';
import { SnackbarService } from '../snackbar/snackbar.service';
import { BlobService } from '../blob/blob.service';
import {
  SasTokenHandler,
  uploadPhoto,
} from '../../features/camera/uploadPhoto';
import { AUTH_TOKEN_STORAGE_KEY } from '../auth/auth.const';

const POLLING_INTERVAL = 3000;

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  private readonly apiBaseUrl;
  private readonly archiveDownloadsContainer;
  private sasUploadToken: SasUri = {
    baseUri: '',
    params: '',
    expiresOn: new Date(0),
  };
  private photoUploadFinishedSubject = new Subject<boolean>();

  public photoUploadFinished$ = this.photoUploadFinishedSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly snackbarService: SnackbarService,
    private readonly blobService: BlobService,
    envService: EnvService,
  ) {
    this.apiBaseUrl = envService.getConfig().apiBaseUrl;
    this.archiveDownloadsContainer =
      envService.getConfig().archiveDownloadsContainer;
  }

  searchPhotos(searchParams: PhotoSearchParamsDto) {
    let params = new HttpParams().append(
      'galleryId',
      searchParams.galleryId.toString(),
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

  bulkDelete(photoIds: number[]) {
    return this.http
      .post<string>(
        `${this.apiBaseUrl}/image/bulk-delete`,
        {
          photoIds,
        },
        {
          ...getAuthHeaders(),
        },
      )
      .pipe(map((x) => Number(x)));
  }

  bulkMovePhotos(photoIds: number[], targetGalleryId: number) {
    console.log(photoIds, targetGalleryId);
    return this.http
      .post<string>(
        `${this.apiBaseUrl}/image/bulk-move`,
        {
          photoIds,
          targetGalleryId,
        },
        {
          ...getAuthHeaders(),
        },
      )
      .pipe(map((x) => Number(x)));
  }

  bulkDownload(photoIds: number[], processed: boolean) {
    return this.http.post<DownloadRequestDto>(
      `${this.apiBaseUrl}/image/bulk-download`,
      {
        photoIds,
        processed,
      },
      {
        ...getAuthHeaders(),
      },
    );
  }

  getUploadSasUri(eventId: number) {
    if (
      this.sasUploadToken &&
      this.sasUploadToken.eventId === eventId &&
      this.sasUploadToken.expiresOn > new Date()
    ) {
      return of(this.sasUploadToken);
    }

    return this.http
      .get<SasUriResponse>(`${this.apiBaseUrl}/image/sas/${eventId}`, {
        ...getAuthHeaders(),
      })
      .pipe(
        map((sasResponse) => {
          const parts = sasResponse.sasUri.split('?');
          this.sasUploadToken.baseUri = parts[0];
          this.sasUploadToken.params = parts[1];
          this.sasUploadToken.expiresOn = new Date(sasResponse.expiresOn);
          this.sasUploadToken.eventId = sasResponse.eventId;
          return this.sasUploadToken;
        }),
      );
  }

  uploadPhotos(photos: File[], eventId: number, galleryId: number) {
    const renamedFiles = new Map<string, File>();
    photos.map((file) => {
      const newName = `${Date.now()}_${file.name}`;
      renamedFiles.set(newName, file);
    });
    const filenames = Array.from(renamedFiles.keys());

    const fileCallback = (filename: string) =>
      new Promise<File>((resolve, reject) => {
        const file = renamedFiles.get(filename);
        if (file) resolve(file);
        else reject(file);
      });

    const sasTokenHandler: SasTokenHandler = {
      getToken: (_, eventId) => {
        return firstValueFrom(this.getUploadSasUri(eventId));
      },
    };

    const snackbarId = this.snackbarService.addSnackbar(
      SnackbarType.Loading,
      'Nuotraukos įkeliamos',
      false,
    );

    uploadPhoto({
      filenames,
      eventId,
      galleryId,
      fileCallback: fileCallback,
      authToken: localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)!,
      captureDate: new Date(),
      apiBaseUrl: this.apiBaseUrl,
      sasTokenHandler,
    }).then((batch) => {
      if (!batch) {
        this.snackbarService.deleteSnackbar(snackbarId);
        this.snackbarService.addSnackbar(
          SnackbarType.Error,
          'Įvyko klaida įkeliant nuotraukas.',
        );
        return;
      }
      this.startUploadTask(batch.id, snackbarId);
    });
  }

  startDownloadTask(requestId: number, snackbarId: number) {
    this.http
      .get<DownloadRequestDto>(
        `${this.apiBaseUrl}/image/archive-download/${requestId}`,
        { ...getAuthHeaders() },
      )
      .pipe(
        repeat({ delay: POLLING_INTERVAL }),
        skipWhile((download) => !download.isReady),
        take(1),
        switchMap((downloadReq) => this.downloadFile(downloadReq, snackbarId)),
      )
      .subscribe();
  }

  private downloadFile(downloadReq: DownloadRequestDto, snackbarId: number) {
    return this.blobService
      .getFromBlob(this.archiveDownloadsContainer, downloadReq.filename)
      .pipe(
        tap((archiveBlob) => {
          this.snackbarService.deleteSnackbar(snackbarId);
          this.snackbarService.addSnackbar(
            SnackbarType.Success,
            'Nuotraukų archyvas paruoštas.',
          );

          const archiveUri = URL.createObjectURL(archiveBlob);
          const a = document.createElement('a');
          a.href = archiveUri;
          a.download = downloadReq.filename;
          a.click();
          console.log(a);
          console.log(downloadReq);
          URL.revokeObjectURL(archiveUri);
        }),
      );
  }

  private startUploadTask(requestId: number, snackbarId: number) {
    this.http
      .get<BatchResponse>(`${this.apiBaseUrl}/image/batch/${requestId}`, {
        ...getAuthHeaders(),
      })
      .pipe(
        repeat({ delay: POLLING_INTERVAL }),
        skipWhile((batch) => !batch.ready),
        take(1),
        tap(() => {
          this.snackbarService.deleteSnackbar(snackbarId);
          this.snackbarService.addSnackbar(
            SnackbarType.Success,
            'Nuotraukos įkeltos',
          );
          this.photoUploadFinishedSubject.next(true);
        }),
      )
      .subscribe();
  }
}
