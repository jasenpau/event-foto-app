import { Injectable } from '@angular/core';
import { ImageService } from '../image/image.service';
import { BlobService } from '../blob/blob.service';
import { repeat, skipWhile, switchMap, take, tap } from 'rxjs';
import { SnackbarService } from '../snackbar/snackbar.service';
import { EnvService } from '../environment/env.service';
import { DownloadRequestDto } from '../image/image.types';
import { SnackbarType } from '../snackbar/snackbar.types';

const POLLING_INTERVAL = 3000;

@Injectable({
  providedIn: 'root',
})
export class BackgroundTasksService {
  private readonly archiveDownloadsContainer;

  constructor(
    private readonly imageService: ImageService,
    private readonly blobService: BlobService,
    private readonly snackbarService: SnackbarService,
    readonly env: EnvService,
  ) {
    this.archiveDownloadsContainer = env.getConfig().archiveDownloadsContainer;
  }

  startDownloadTask(requestId: number) {
    const snackbarId = this.snackbarService.addSnackbar(
      SnackbarType.Downloading,
      'Nuotraukų archyvas yra ruošiamas.',
      false,
    );
    this.imageService
      .getDownloadRequestStatus(requestId)
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
}
