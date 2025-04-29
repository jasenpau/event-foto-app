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
import { ActivatedRoute, Router } from '@angular/router';
import {
  OpenPhotoData,
  PhotoAction,
  PhotoListDto,
  SasUri,
} from '../../../services/image/image.types';
import { ImageService } from '../../../services/image/image.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import {
  BehaviorSubject,
  filter,
  forkJoin,
  of,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';
import { PhotoViewComponent } from '../photo-view/photo-view.component';
import { ModalService } from '../../../services/modal/modal.service';
import { PluralDefinition, pluralizeLt } from '../../../helpers/pluralizeLt';
import { ModalActions } from '../../../services/modal/modal.types';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { BlobService } from '../../../services/blob/blob.service';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { LoaderService } from '../../../services/loader/loader.service';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { CreateEditGalleryFormComponent } from '../../events/create-gallery-form/create-edit-gallery-form.component';
import { handleApiError } from '../../../helpers/handleApiError';
import { MovePhotosFormComponent } from '../move-photos-form/move-photos-form.component';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { UploadPhotoFormComponent } from '../upload-photo-form/upload-photo-form.component';
import { EMPTY_SUBSCRIPTION } from 'rxjs/internal/Subscription';
import { IconButtonComponent } from '../../../components/icon-button/icon-button.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import {
  PopupMenuComponent,
  PopupMenuItem,
} from '../../../components/popup-menu/popup-menu.component';
import { ViewPermissions } from '../../../globals/userGroups';
import { UserService } from '../../../services/user/user.service';

const COMPONENT_LOADING_KEY = 'gallery-view';

@Component({
  selector: 'app-gallery-view',
  imports: [
    ButtonComponent,
    NgForOf,
    NgIf,
    PhotoViewComponent,
    NgClass,
    SpinnerComponent,
    SideViewComponent,
    CreateEditGalleryFormComponent,
    MovePhotosFormComponent,
    UploadPhotoFormComponent,
    IconButtonComponent,
    PageHeaderComponent,
    PopupMenuComponent,
  ],
  templateUrl: './gallery-view.component.html',
  styleUrl: './gallery-view.component.scss',
})
export class GalleryViewComponent
  extends DisposableComponent
  implements OnDestroy, AfterViewInit
{
  @ViewChild('scrollAnchor', { static: false }) scrollAnchor?: ElementRef;

  protected eventId?: number;
  protected galleryId?: number;
  protected galleryDetails?: GalleryDto;
  protected imageData: PhotoListDto[] = [];
  protected selectedImageIds = new Set<number>();
  protected hasMoreImages = true;
  protected isLoading = false;
  protected openedPhotoData?: OpenPhotoData;
  protected sasUri?: SasUri;
  protected showGalleryEditForm = false;
  protected showMovePhotosForm = false;
  protected downloadOptions: PopupMenuItem[] = [
    {
      text: 'Originalios nuotraukos',
      action: () => {
        this.bulkDownload(false);
      },
    },
    {
      text: 'Apdirbtos nuotraukos',
      action: () => {
        this.bulkDownload(true);
      },
    },
  ];
  protected showDownloadOptions = false;
  protected permissionViews?: ViewPermissions;

  private lastKey = '';
  private observer?: IntersectionObserver;
  private viewLoaded = new BehaviorSubject(false);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly imageService: ImageService,
    private readonly modalService: ModalService,
    private readonly blobService: BlobService,
    private readonly galleryService: GalleryService,
    private readonly loaderService: LoaderService,
    private readonly snackbarService: SnackbarService,
    private readonly userService: UserService,
    private readonly router: Router,
  ) {
    super();
    this.loaderService.startLoading(COMPONENT_LOADING_KEY);
    this.readRouteParams();
    this.permissionViews = this.userService.getViewPermissions();
  }

  ngAfterViewInit(): void {
    this.viewLoaded.next(true);
  }

  protected loadMore() {
    if (
      this.galleryId &&
      this.eventId &&
      this.hasMoreImages &&
      !this.isLoading
    ) {
      this.isLoading = true;

      const sas = this.blobService.getReadOnlySasUri();

      const imageData = this.imageService.searchPhotos({
        keyOffset: this.lastKey === '' ? null : this.lastKey,
        galleryId: this.galleryId,
      });

      return forkJoin([imageData, sas])
        .pipe(
          tap(([images, sas]) => {
            this.sasUri = sas;
            this.imageData.push(...images.data);
            this.hasMoreImages = images.hasNextPage;
            if (images.data.length > 0) {
              this.lastKey = `${images.data[images.data.length - 1].captureDate}|${images.data[images.data.length - 1].id}`;
            }
            this.isLoading = false;
            this.setupObserver();
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }

    return EMPTY_SUBSCRIPTION;
  }

  protected openPhoto(image: PhotoListDto, index: number) {
    if (this.eventId) {
      this.openedPhotoData = {
        photo: image,
        eventId: this.eventId,
        ...this.showControls(index),
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

  protected handlePhotoClick(
    $event: MouseEvent,
    image: PhotoListDto,
    index: number,
  ) {
    if ($event.ctrlKey) {
      this.togglePhotoSelect(image);
    } else {
      this.openPhoto(image, index);
    }
  }

  protected handlePhotoKeyboard(
    $event: KeyboardEvent,
    image: PhotoListDto,
    index: number,
  ) {
    if ($event.key === 'Enter') {
      this.openPhoto(image, index);
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
        return;
      case PhotoAction.Delete:
        this.openedPhotoData = undefined;
        this.reload();
        return;
      case PhotoAction.Next:
        this.shiftOpenedImageIndex(1);
        return;
      case PhotoAction.Previous:
        this.shiftOpenedImageIndex(-1);
        return;
    }
  }

  private shiftOpenedImageIndex(offset: number) {
    const photo = this.openedPhotoData?.photo;
    if (photo) {
      const index = this.imageData.indexOf(photo);
      if (index === -1) return;

      const newIndex = index + offset;
      if (newIndex >= 0 && newIndex < this.imageData.length) {
        this.openedPhotoData = {
          eventId: this.eventId!,
          photo: this.imageData[newIndex],
          ...this.showControls(newIndex),
        };
      } else if (newIndex == this.imageData.length && this.hasMoreImages) {
        this.loadMore().add(() => {
          this.openedPhotoData = {
            eventId: this.eventId!,
            photo: this.imageData[newIndex],
            ...this.showControls(newIndex),
          };
        });
      }
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
          if (result === ModalActions.Confirm) {
            this.imageData = [];
            this.isLoading = true;
            return this.imageService.bulkDelete(selectedImages).pipe(
              tap(() => {
                this.selectedImageIds.clear();
                this.reload();
                this.snackbarService.addSnackbar(
                  SnackbarType.Info,
                  'Nuotraukos ištrintos',
                );
              }),
            );
          }
          return of({});
        }),
      )
      .subscribe();
  }

  protected openGalleryEditForm() {
    this.showGalleryEditForm = true;
  }

  protected openMovePhotosForm() {
    this.showMovePhotosForm = true;
  }

  protected handlePhotoUpload() {
    this.imageService.photoUploadFinished$
      .pipe(
        tap((reload) => {
          if (reload) window.location.reload();
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  protected handleGalleryEditFormEvent($event: string) {
    if ($event === 'cancel') {
      this.showGalleryEditForm = false;
    } else if ($event === 'updated') {
      this.showGalleryEditForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Galerija atnaujinta.',
      );
      this.getGalleryDetails(this.galleryId!);
    }
  }

  protected handleMovePhotosFormEvent(event: string) {
    if (event === 'cancel') {
      this.showMovePhotosForm = false;
    } else if (event === 'moved') {
      this.showMovePhotosForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Nuotraukos perkeltos.',
      );
      this.reload();
    }
  }

  protected deleteGallery() {
    this.galleryService
      .deleteGallery(this.galleryId!)
      .pipe(
        tap(() => {
          this.snackbarService.addSnackbar(
            SnackbarType.Info,
            'Galerija ištrinta.',
          );
          this.router.navigate(['/event', this.eventId!]);
        }),
        handleApiError((error) => {
          if (error.title === 'default-gallery') {
            this.snackbarService.addSnackbar(
              SnackbarType.Error,
              'Negalima ištrinti pagrindinės renginio galerijos',
            );
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const galleryId = Number(params.get('galleryId'));
      if (!isNaN(galleryId) && galleryId > 0) {
        this.galleryId = galleryId;
        this.getGalleryDetails(galleryId);
      }
    });
  }

  private getGalleryDetails(galleryId: number) {
    const gallery$ = this.galleryService.getGalleryDetails(galleryId);
    const viewLoaded$ = this.viewLoaded.pipe(
      filter((value) => value),
      take(1),
    );

    forkJoin([gallery$, viewLoaded$])
      .pipe(
        tap(([gallery]) => {
          this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
          this.galleryDetails = gallery;
          this.eventId = gallery.eventId;
          this.setupObserver();
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private setupObserver() {
    if (this.observer) this.observer.disconnect();
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

  private bulkDownload(processed: boolean) {
    const snackbarId = this.snackbarService.addSnackbar(
      SnackbarType.Loading,
      'Nuotraukų archyvas yra ruošiamas.',
      false,
    );
    const selectedImages = Array.from(this.selectedImageIds);
    this.imageService
      .bulkDownload(selectedImages, processed)
      .pipe(
        tap((downloadReq) => {
          this.imageService.startDownloadTask(downloadReq.id, snackbarId);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private showControls(index: number) {
    return {
      showNext: index < this.imageData.length - 1 || this.hasMoreImages,
      showPrevious: index > 0,
    };
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.observer?.disconnect();
    this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
    this.viewLoaded.complete();
  }

  protected readonly ButtonType = ButtonType;
  protected readonly SvgIconSrc = SvgIconSrc;
}
