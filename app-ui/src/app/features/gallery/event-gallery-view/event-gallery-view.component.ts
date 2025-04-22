import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
} from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgClass, NgForOf, NgIf } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import {
  OpenPhotoData,
  PhotoAction,
  PhotoListDto,
  ReadOnlySasUri,
} from '../../../services/image/image.types';
import { ImageService } from '../../../services/image/image.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { forkJoin, of, switchMap, takeUntil, tap } from 'rxjs';
import { PhotoViewComponent } from '../photo-view/photo-view.component';
import { ModalService } from '../../../services/modal/modal.service';
import { PluralDefinition, pluralizeLt } from '../../../helpers/pluralizeLt';
import { ModalActions } from '../../../services/modal/modal.types';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { BlobService } from '../../../services/blob/blob.service';
import { BackgroundTasksService } from '../../../services/background-tasks/background-tasks.service';

@Component({
  selector: 'app-event-gallery-view',
  imports: [
    ButtonComponent,
    NgForOf,
    NgIf,
    PhotoViewComponent,
    NgClass,
    SpinnerComponent,
  ],
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
  protected selectedImageIds = new Set<number>();
  protected hasMoreImages = true;
  protected isLoading = false;
  protected openedPhotoData?: OpenPhotoData;
  protected sasUri?: ReadOnlySasUri;

  private lastKey = '';
  private observer?: IntersectionObserver;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly imageService: ImageService,
    private readonly modalService: ModalService,
    private readonly blobService: BlobService,
    private readonly backgroundTasksService: BackgroundTasksService,
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

      const sas = this.blobService.getReadOnlySasUri();

      const imageData = this.imageService.searchPhotos({
        keyOffset: this.lastKey === '' ? null : this.lastKey,
        eventId: this.eventId,
      });

      forkJoin([imageData, sas])
        .pipe(
          tap(([images, sas]) => {
            this.sasUri = sas;
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

  protected togglePhotoSelect(image: PhotoListDto) {
    if (this.selectedImageIds.has(image.id)) {
      this.selectedImageIds.delete(image.id);
    } else {
      this.selectedImageIds.add(image.id);
    }
  }

  protected handlePhotoClick(image: PhotoListDto) {
    if (this.selectedImageIds.size === 0) this.openPhoto(image);
    else this.togglePhotoSelect(image);
  }

  protected handlePhotoKeyboard($event: KeyboardEvent, image: PhotoListDto) {
    if ($event.key === 'Enter') {
      this.openPhoto(image);
    } else if ($event.key === ' ') {
      // handle Space
      $event.preventDefault();
      this.togglePhotoSelect(image);
    }
  }

  protected handlePhotoViewAction(event: PhotoAction) {
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

  protected unmarkAll() {
    this.selectedImageIds.clear();
  }

  protected markAll() {
    this.selectedImageIds.clear();
    this.imageData.forEach((i) => this.selectedImageIds.add(i.id));
  }

  protected bulkDelete() {
    const selectedImages = Array.from(this.selectedImageIds);
    const photoDefinition: PluralDefinition = {
      singular: 'nuotrauką',
      smallPlural: 'nuotraukas',
      largePlural: 'nuotraukų',
    };

    this.modalService
      .openConfirmModal({
        body: `Ar tikrai norite ištrinti ${pluralizeLt(selectedImages.length, photoDefinition)}?`,
        confirm: 'Trinti',
      })
      .pipe(
        switchMap((result) => {
          console.log('result');
          if (result === ModalActions.Confirm) {
            this.imageData = [];
            this.isLoading = true;
            return this.imageService.bulkDelete(selectedImages).pipe(
              tap(() => {
                this.selectedImageIds.clear();
                this.reload();
              }),
            );
          }
          return of({});
        }),
      )
      .subscribe();
  }

  protected bulkDownload() {
    const selectedImages = Array.from(this.selectedImageIds);
    this.imageService
      .bulkDownload(selectedImages)
      .pipe(
        tap((downloadReq) => {
          this.backgroundTasksService.startDownloadTask(downloadReq.id);
          this.selectedImageIds.clear();
          this.reload();
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
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
}
