import {
  AfterViewInit,
  Component,
  effect,
  ElementRef,
  EventEmitter,
  input,
  OnDestroy,
  Output,
  ViewChild,
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
import { forkJoin, fromEvent, Observable, of, takeUntil, tap } from 'rxjs';
import { NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../components/button/button.component';
import { formatLithuanianDateWithSeconds } from '../../../helpers/formatLithuanianDate';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { IconButtonComponent } from '../../../components/icon-button/icon-button.component';
import { BlobService } from '../../../services/blob/blob.service';
import { useLocalLoader } from '../../../helpers/useLoader';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';

@Component({
  selector: 'app-photo-view',
  imports: [
    NgIf,
    RouterLink,
    ButtonComponent,
    IconButtonComponent,
    LoaderOverlayComponent,
  ],
  templateUrl: './photo-view.component.html',
  styleUrl: './photo-view.component.scss',
})
export class PhotoViewComponent
  extends DisposableComponent
  implements OnDestroy, AfterViewInit
{
  // @Input({ required: true }) openPhotoData!: OpenPhotoData;
  openPhotoData = input.required<OpenPhotoData>();
  externalLoader = input.required<boolean>();
  @Output() photoAction = new EventEmitter<PhotoAction>();

  @ViewChild('photoOverlay') photoOverlay!: ElementRef;

  protected photoDetails?: PhotoDetailDto;
  protected imageDataUrl?: string;
  protected isLoading = true;

  constructor(
    private readonly imageService: ImageService,
    private readonly snackbarService: SnackbarService,
    private readonly blobService: BlobService,
  ) {
    super();
    effect(() => {
      this.loadImage(this.openPhotoData());
    });
  }

  ngAfterViewInit() {
    fromEvent<KeyboardEvent>(document.body, 'keyup')
      .pipe(
        tap((event) => {
          this.handleKeyboardEvent(event);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
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

  protected overlayClick($event: MouseEvent) {
    if ($event.target === this.photoOverlay.nativeElement) {
      this.close();
    }
  }

  protected nextNavigate() {
    this.photoAction.emit(PhotoAction.Next);
  }

  protected previousNavigate() {
    this.photoAction.emit(PhotoAction.Previous);
  }

  private handleKeyboardEvent($event: KeyboardEvent) {
    switch ($event.key) {
      case 'Escape':
        this.close();
        return;
      case 'ArrowLeft':
        this.previousNavigate();
        return;
      case 'ArrowRight':
        this.nextNavigate();
        return;
    }
  }

  private loadImage(openPhotoData: OpenPhotoData) {
    this.isLoading = true;

    const details$ = this.imageService
      .getPhotoDetails(openPhotoData.photo.id)
      .pipe(
        tap((details) => {
          this.photoDetails = details;
        }),
        takeUntil(this.destroy$),
      );

    let image$: Observable<Blob | null> = of(null);
    if (
      openPhotoData.photo.isProcessed &&
      openPhotoData.photo.processedFilename
    ) {
      image$ = this.blobService
        .getFromBlob(
          `event-${openPhotoData.eventId}`,
          openPhotoData.photo.processedFilename,
        )
        .pipe(
          tap((blob) => {
            this.imageDataUrl = URL.createObjectURL(blob);
          }),
          takeUntil(this.destroy$),
        );
    }

    forkJoin({
      detailsSubscription: details$,
      imageSubscription: image$,
    })
      .pipe(useLocalLoader((value) => (this.isLoading = value)))
      .subscribe();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    if (this.imageDataUrl) URL.revokeObjectURL(this.imageDataUrl);
  }

  protected readonly SvgIconSrc = SvgIconSrc;
  protected readonly ButtonType = ButtonType;
}
