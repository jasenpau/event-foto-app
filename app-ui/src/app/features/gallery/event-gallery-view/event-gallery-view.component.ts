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
import { PhotoListDto } from '../../../services/gallery/gallery.types';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { ThumbnailBaseUrl } from '../../../globals/variables';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-event-gallery-view',
  imports: [ButtonComponent, NgForOf, NgIf],
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

  private lastKey = '';
  private observer?: IntersectionObserver;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly galleryService: GalleryService,
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
            this.lastKey = `${images.data[images.data.length - 1].captureDate}|${images.data[images.data.length - 1].id}`;
            this.isLoading = false;
            this.observer?.disconnect();
            this.setupObserver();
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const eventId = Number(params.get('eventId'));
      if (!isNaN(eventId) && eventId > 0) {
        this.eventId = eventId;
        this.initialize();
      }
    });
  }

  private initialize() {
    // this.loadMore();
  }

  private setupObserver() {
    this.observer = new IntersectionObserver(
      (entries) => {
        console.log('checking', entries);
        console.log(
          entries[0].isIntersecting,
          this.isLoading,
          this.hasMoreImages,
        );
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

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.observer?.disconnect();
  }

  protected readonly ButtonType = ButtonType;
  protected readonly ThumbnailBaseUrl = ThumbnailBaseUrl;
}
