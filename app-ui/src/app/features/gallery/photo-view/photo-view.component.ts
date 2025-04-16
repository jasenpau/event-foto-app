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
import { AppSvgIconComponent } from '../../../components/svg-icon/app-svg-icon.component';
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

@Component({
  selector: 'app-photo-view',
  imports: [AppSvgIconComponent, NgIf, RouterLink],
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

  constructor(private readonly imageService: ImageService) {
    super();
  }

  ngAfterViewInit() {
    this.loadImage();
  }

  protected close() {
    this.photoAction.emit(PhotoAction.Close);
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
      imageSubscription = this.imageService
        .getRawPhoto(
          this.openPhotoData.eventId,
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

  protected readonly SvgIconSrc = SvgIconSrc;
  protected readonly ButtonType = ButtonType;
}
