import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
} from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import {
  OpenPhotoData,
  PhotoAction,
  PhotoListDto,
} from '../../../services/image/image.types';
import { ImageService } from '../../../services/image/image.service';
import { ThumbnailBaseUrl } from '../../../globals/variables';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { PhotoViewComponent } from '../photo-view/photo-view.component';

@Component({
  selector: 'app-event-gallery-view',
  imports: [ButtonComponent, NgForOf, NgIf, PhotoViewComponent],
  templateUrl: './event-gallery-view.component.html',
  styleUrl: './event-gallery-view.component.scss',
})
export class EventGalleryViewComponent
  extends DisposableComponent
  implements OnDestroy, AfterViewInit
{
  @ViewChild('scrollAnchor', { static: false }) scrollAnchor?: ElementRef;

  protected eventId?: number;
  protected imageData: PhotoListDto[] = [];
  protected hasMoreImages = true;
  protected isLoading = false;
  protected openedPhotoData?: OpenPhotoData;

  private lastKey = '';
  private observer?: IntersectionObserver;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly galleryService: ImageService,
  ) {
    super();
    this.readRouteParams();
  }

  ngAfterViewInit(): void {
    if (this.scrollAnchor) {
      this.setupObserver();
    }
  }

  protected loadMore() {
    if (this.eventId && this.hasMoreImages && !this.isLoading) {
      this.isLoading = true;

      this.galleryService
        .searchPhotos({
          keyOffset: this.lastKey === '' ? null : this.lastKey,
          eventId: this.eventId,
        })
        .pipe(
          tap((images) => {
            this.imageData.push(...images.data);
            this.hasMoreImages = images.hasNextPage;
            if (images.data.length > 0) {
              this.lastKey = `${images.data[images.data.length - 1].captureDate}|${images.data[images.data.length - 1].id}`;
            }
            this.isLoading = false;
            this.observer?.disconnect();
            this.setupObserver();
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected openPhoto(image: PhotoListDto) {
    if (this.eventId) {
      this.openedPhotoData = {
        photo: image,
        eventId: this.eventId,
      };
    }
  }

  handlePhotoViewAction(event: PhotoAction) {
    switch (event) {
      case PhotoAction.Close:
        this.openedPhotoData = undefined;
        break;
      case PhotoAction.Delete:
        this.openedPhotoData = undefined;
        this.reload();
        break;
    }
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const eventId = Number(params.get('eventId'));
      if (!isNaN(eventId) && eventId > 0) {
        this.eventId = eventId;
      }
    });
  }

  private setupObserver() {
    this.observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        if (entry.isIntersecting && !this.isLoading && this.hasMoreImages) {
          this.loadMore();
        }
      },
      {
        root: null,
        threshold: 1,
      },
    );
    this.observer.observe(this.scrollAnchor!.nativeElement);
  }

  private reload() {
    this.hasMoreImages = true;
    this.imageData = [];
    this.isLoading = false;
    this.lastKey = '';
    this.loadMore();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.observer?.disconnect();
  }

  protected readonly ButtonType = ButtonType;
  protected readonly ThumbnailBaseUrl = ThumbnailBaseUrl;
}
