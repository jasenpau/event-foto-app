import { Component, OnDestroy, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import {
  EventDto,
  PhotographerAssignment,
} from '../../../services/event/event.types';
import { NgForOf, NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ActivatedRoute, Router } from '@angular/router';
import { delay, forkJoin, of, switchMap, takeUntil, tap } from 'rxjs';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { EventBadgeComponent } from '../../../components/event-badge/event-badge.component';
import { ViewPermissions } from '../../../globals/userGroups';
import { UserService } from '../../../services/user/user.service';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { AssignPhotographerFormComponent } from '../assign-photographer-form/assign-photographer-form.component';
import { LoaderService } from '../../../services/loader/loader.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { useLoader, useLocalLoader } from '../../../helpers/useLoader';
import { CardGridComponent } from '../../../components/cards/card-grid/card-grid.component';
import { CardItemComponent } from '../../../components/cards/card-item/card-item.component';
import { PluralDefinition, pluralizeLt } from '../../../helpers/pluralizeLt';
import { SasUri } from '../../../services/image/image.types';
import { BlobService } from '../../../services/blob/blob.service';
import { CreateEditGalleryFormComponent } from '../create-gallery-form/create-edit-gallery-form.component';
import { AppSvgIconComponent } from '../../../components/svg-icon/app-svg-icon.component';
import {
  SvgIconSize,
  SvgIconSrc,
} from '../../../components/svg-icon/svg-icon.types';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { CreateEventComponent } from '../create-event/create-event.component';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { ModalService } from '../../../services/modal/modal.service';
import { ModalActions } from '../../../services/modal/modal.types';
import { EnvService } from '../../../services/environment/env.service';
import { AssignGalleryFormComponent } from '../assign-gallery-form/assign-gallery-form.component';

const COMPONENT_LOADING_KEY = 'event-preview';

interface AssignmentEditData {
  photographerId: string;
  galleryId?: number;
}

@Component({
  selector: 'app-event-preview',
  imports: [
    ReactiveFormsModule,
    NgIf,
    ButtonComponent,
    EventBadgeComponent,
    NgForOf,
    SideViewComponent,
    AssignPhotographerFormComponent,
    CardGridComponent,
    CardItemComponent,
    CreateEditGalleryFormComponent,
    AppSvgIconComponent,
    PageHeaderComponent,
    CreateEventComponent,
    SpinnerComponent,
    AssignGalleryFormComponent,
  ],
  templateUrl: './event-preview.component.html',
  styleUrl: './event-preview.component.scss',
})
export class EventPreviewComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected readonly ButtonType = ButtonType;

  protected event?: EventDto;
  protected eventPhotographers?: PhotographerAssignment[];
  protected isAssignedSelf = false;
  protected permissionViews?: ViewPermissions;
  protected showAssignUsersForm = false;
  protected showGalleryCreateForm = false;
  protected showEventEditForm = false;
  protected assignmentEditData?: AssignmentEditData;
  protected userId?: string;
  protected galleries: GalleryDto[] = [];
  protected sasUri?: SasUri;
  protected assignmentsLoading = false;

  private readonly archiveContainer;

  constructor(
    private readonly eventService: EventService,
    private readonly userService: UserService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly snackbarService: SnackbarService,
    private readonly loaderService: LoaderService,
    private readonly blobService: BlobService,
    private readonly galleryService: GalleryService,
    private readonly modalService: ModalService,
    private envService: EnvService,
  ) {
    super();
    this.loaderService.startLoading(COMPONENT_LOADING_KEY);
    this.readRouteParams();
    this.archiveContainer =
      this.envService.getConfig().archiveDownloadsContainer;
  }

  get isEventArchived() {
    return (
      Boolean(this.event?.isArchived ?? false) ||
      Boolean(this.event?.archiveName ?? false)
    );
  }

  ngOnInit() {
    this.userId = this.userService.getCurrentUserData()?.id;
    this.permissionViews = this.userService.getViewPermissions();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
  }

  protected formatDate(dateString: string): string {
    return formatLithuanianDate(new Date(dateString));
  }

  protected removePhotographer(userId: string) {
    if (this.event && userId) {
      this.eventService
        .unassignPhotographerFromEvent(this.event.id, userId)
        .pipe(
          useLocalLoader((value) => (this.assignmentsLoading = value)),
          tap(() => {
            this.snackbarService.addSnackbar(
              SnackbarType.Success,
              'Fotografas buvo pašalintas',
            );
            this.loadPhotographers(this.event!.id);
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected openPhotographerForm() {
    this.showAssignUsersForm = true;
  }

  protected openGalleryForm() {
    this.showGalleryCreateForm = true;
  }

  protected openEventEditForm() {
    this.showEventEditForm = true;
  }

  protected openGalleryAssignmentForm(
    photographerId: string,
    galleryId?: number,
  ) {
    this.assignmentEditData = { photographerId, galleryId };
  }

  protected archiveEvent() {
    this.modalService
      .openConfirmModal({
        body: 'Ar tikrai norite archyvuoti šį renginį?\nRenginio originalio nuotraukos bus pašalintos ir bus galima tik atsisiųsti visas apdorotas nuotraukas.',
        confirm: 'Archyvuoti',
        cancel: 'Atšaukti',
      })
      .pipe(
        switchMap((modalAction) => {
          if (modalAction === ModalActions.Confirm) {
            this.loaderService.startLoading(`${COMPONENT_LOADING_KEY}_archive`);
            return this.eventService.archiveEvent(this.event!.id).pipe(
              delay(3000),
              tap(() => {
                this.snackbarService.addSnackbar(
                  SnackbarType.Success,
                  'Renginys bus archyvuotas artimiausiu metu, prašome palaukti',
                );
                this.loaderService.finishLoading(
                  `${COMPONENT_LOADING_KEY}_archive`,
                );
                this.router.navigate(['/event']);
              }),
            );
          } else return of(null);
        }),
      )
      .subscribe();
  }

  protected downloadEventArchive() {
    if (this.event?.isArchived && this.event.archiveName) {
      this.blobService
        .getFromBlob(this.archiveContainer, this.event.archiveName)
        .pipe(
          tap((blob) => {
            const originalUri = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = originalUri;
            a.download = this.event!.archiveName!;
            a.click();
            URL.revokeObjectURL(originalUri);
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected deleteEvent() {
    if (!this.event) return;

    const deleteEvent$ = this.eventService.deleteEvent(this.event.id).pipe(
      useLoader(`${COMPONENT_LOADING_KEY}_delete`, this.loaderService),
      tap(() => {
        this.snackbarService.addSnackbar(
          SnackbarType.Info,
          'Renginys ištrintas',
        );
        this.router.navigate(['/event']);
      }),
    );

    this.modalService
      .openConfirmModal({
        body: 'Ar tikrai norite ištrinti šį renginį?\nŠis veiksmas yra negrįžtamas.',
        confirm: 'Ištrinti',
        cancel: 'Atšaukti',
      })
      .pipe(
        switchMap((modalAction) => {
          return modalAction === ModalActions.Confirm ? deleteEvent$ : of(null);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  protected handlePhotographerFormEvent($event: string) {
    if ($event === 'cancel') {
      this.showAssignUsersForm = false;
    } else if ($event === 'assigned') {
      this.showAssignUsersForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Fotografui priskirta galerija',
      );
      this.loadPhotographers(this.event!.id);
    }
  }

  protected handleGalleryAssignment($event: string) {
    if ($event === 'cancel') {
      this.assignmentEditData = undefined;
    } else if ($event === 'assigned') {
      this.assignmentEditData = undefined;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Fotografui priskirta galerija',
      );
      this.loadPhotographers(this.event!.id);
    }
  }

  protected handleGalleryCreateFormEvent($event: string) {
    if ($event === 'cancel') {
      this.showGalleryCreateForm = false;
    } else if ($event === 'created') {
      this.showGalleryCreateForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Galerija sukurta',
      );
      this.loadGalleries(this.event!.id);
    }
  }

  protected handleEventEditFormEvent($event: string) {
    if ($event === 'cancel') {
      this.showEventEditForm = false;
    } else if ($event === 'updated') {
      this.showEventEditForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Renginys atnaujintas',
      );
      this.loadEvent(this.event!.id);
    }
  }

  protected getGalleryThumbnail(gallery: GalleryDto) {
    if (gallery.thumbnail)
      return `${this.sasUri?.baseUri}/event-${gallery.eventId}/thumb-${gallery.thumbnail}?${this.sasUri?.params}`;

    return `/assets/default-cover.jpg`;
  }

  protected getPhotoCountString(photoCount: number) {
    if (photoCount === 0) return 'Nėra nuotraukų';

    const photoDefinition: PluralDefinition = {
      singular: 'nuotrauka',
      smallPlural: 'nuotraukos',
      largePlural: 'nuotraukų',
    };
    return pluralizeLt(photoCount, photoDefinition);
  }

  protected openGallery(gallery: GalleryDto) {
    this.router.navigate(['/gallery', gallery.id]);
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('eventId'));
      if (!isNaN(id) && id > 0) {
        this.loadEvent(id);
        this.loadPhotographers(id);
        this.loadGalleries(id);
      }
    });
  }

  private loadEvent(eventId: number) {
    this.eventService
      .getEventDetails(eventId)
      .pipe(
        tap((event) => {
          this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
          this.event = event;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private loadPhotographers(eventId: number) {
    this.eventService
      .getEventPhotographers(eventId)
      .pipe(
        useLocalLoader((value) => (this.assignmentsLoading = value)),
        tap((data) => {
          this.setPhotographerList(data);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private loadGalleries(eventId: number) {
    const galleries$ = this.galleryService.getEventGalleries(eventId);
    const sas$ = this.blobService.getReadOnlySasUri();

    forkJoin([galleries$, sas$])
      .pipe(
        useLoader(`${COMPONENT_LOADING_KEY}_galleries`, this.loaderService),
        tap(([galleries, sas]) => {
          this.galleries = galleries;
          this.sasUri = sas;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private setPhotographerList(assignment: PhotographerAssignment[]) {
    this.eventPhotographers = assignment;
    this.isAssignedSelf = this.userId
      ? assignment.some((u) => this.userId === u.userId)
      : false;
  }

  protected readonly SvgIconSrc = SvgIconSrc;
  protected readonly SvgIconSize = SvgIconSize;
}
