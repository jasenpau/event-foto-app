import {
  AfterViewInit,
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
} from '@angular/core';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { ButtonType } from '../../../components/button/button.types';
import {
  OpenPhotoData,
  PhotoAction,
  PhotoDetailDto,
} from '../../../services/image/image.types';
import { ImageService } from '../../../services/image/image.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { forkJoin, Observable, of, takeUntil, tap } from 'rxjs';
import { NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../components/button/button.component';
import { formatLithuanianDateWithSeconds } from '../../../helpers/formatLithuanianDate';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { IconButtonComponent } from '../../../components/icon-button/icon-button.component';
import { BlobService } from '../../../services/blob/blob.service';

@Component({
  selector: 'app-photo-view',
  imports: [NgIf, RouterLink, ButtonComponent, IconButtonComponent],
  templateUrl: './photo-view.component.html',
  styleUrl: './photo-view.component.scss',
})
export class PhotoViewComponent
  extends DisposableComponent
  implements OnDestroy, AfterViewInit
{
  @Input({ required: true }) openPhotoData!: OpenPhotoData;
  @Input() showEventNavigation = false;
  @Output() photoAction = new EventEmitter<PhotoAction>();

  protected photoDetails?: PhotoDetailDto;
  protected imageDataUrl?: string;
  protected isLoading = true;

  constructor(
    private readonly imageService: ImageService,
    private readonly snackbarService: SnackbarService,
    private readonly blobService: BlobService,
  ) {
    super();
  }

  ngAfterViewInit() {
    this.loadImage();
  }

  protected close() {
    this.photoAction.emit(PhotoAction.Close);
  }

  protected deletePhoto() {
    if (!this.photoDetails) return;

    this.imageService
      .bulkDelete([this.photoDetails.id])
      .pipe(
        tap(() => {
          this.snackbarService.addSnackbar(
            SnackbarType.Info,
            'Nuotrauka buvo ištrinta.',
          );
          this.photoAction.emit(PhotoAction.Delete);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  protected downloadPhoto() {
    if (!this.imageDataUrl || !this.photoDetails) return;

    const a = document.createElement('a');
    a.href = this.imageDataUrl;
    a.download = this.photoDetails.processedFilename ?? 'image.jpg';
    a.click();
  }

  protected formatLtDate(dateString: string) {
    return formatLithuanianDateWithSeconds(new Date(dateString));
  }

  private loadImage() {
    if (!this.openPhotoData) return;

    this.isLoading = true;

    const detailsSubscription = this.imageService
      .getPhotoDetails(this.openPhotoData.photo.id)
      .pipe(
        tap((details) => {
          this.photoDetails = details;
        }),
        takeUntil(this.destroy$),
      );

    let imageSubscription: Observable<Blob | null> = of(null);
    if (
      this.openPhotoData.photo.isProcessed &&
      this.openPhotoData.photo.processedFilename
    ) {
      imageSubscription = this.blobService
        .getFromBlob(
          `event-${this.openPhotoData.eventId}`,
          this.openPhotoData.photo.processedFilename,
        )
        .pipe(
          tap((blob) => {
            this.imageDataUrl = URL.createObjectURL(blob);
          }),
          takeUntil(this.destroy$),
        );
    }

    forkJoin({
      detailsSubscription,
      imageSubscription,
    })
      .pipe(
        tap(() => {
          this.isLoading = false;
        }),
      )
      .subscribe();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    if (this.imageDataUrl) URL.revokeObjectURL(this.imageDataUrl);
  }

  protected readonly SvgIconSrc = SvgIconSrc;
  protected readonly ButtonType = ButtonType;
}
