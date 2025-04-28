import { Component, OnDestroy, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import {
  EventDto,
  EventPhotographer,
} from '../../../services/event/event.types';
import { NgForOf, NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, takeUntil, tap } from 'rxjs';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { EventBadgeComponent } from '../../../components/event-badge/event-badge.component';
import { UserGroup } from '../../../globals/userGroups';
import { UserService } from '../../../services/user/user.service';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { AssignPhotographerFormComponent } from '../assign-photographer-form/assign-photographer-form.component';
import { LoaderService } from '../../../services/loader/loader.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { useLoader } from '../../../helpers/useLoader';
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

const COMPONENT_LOADING_KEY = 'event-preview';

@Component({
  selector: 'app-event-preview',
  imports: [
    ReactiveFormsModule,
    NgIf,
    RouterLink,
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
  protected eventPhotographers?: EventPhotographer[];
  protected showAssignSelf = false;
  protected isAssignedSelf = false;
  protected showAssignUsers = false;
  protected showAssignUsersForm = false;
  protected showGalleryCreateForm = false;
  protected showEventEditForm = false;
  protected userId?: string;
  protected galleries: GalleryDto[] = [];
  protected sasUri?: SasUri;

  constructor(
    private readonly eventService: EventService,
    private readonly userService: UserService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly snackbarService: SnackbarService,
    private readonly loaderService: LoaderService,
    private readonly blobService: BlobService,
    private readonly galleryService: GalleryService,
  ) {
    super();
    this.loaderService.startLoading(COMPONENT_LOADING_KEY);
    this.readRouteParams();
  }

  ngOnInit() {
    this.userId = this.userService.getCurrentUserData()?.id;
    this.updateViewPermissions();
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
          tap((data) => {
            this.setPhotographerList(data);
            this.snackbarService.addSnackbar(
              SnackbarType.Success,
              'Fotografas buvo pašalintas.',
            );
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected assignSelf() {
    if (this.event && this.userId) {
      this.eventService
        .assignPhotographerToEvent(this.event.id, this.userId)
        .pipe(
          tap((data) => {
            this.setPhotographerList(data);
            this.snackbarService.addSnackbar(
              SnackbarType.Success,
              'Jūs buvote pridėtas prie renginio kaip fotografas.',
            );
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

  protected handlePhotographerFormEvent($event: string) {
    if ($event === 'cancel') {
      this.showAssignUsersForm = false;
    } else if ($event === 'assigned') {
      this.showAssignUsersForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Fotografas pridėtas prie renginio.',
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
        'Galerija sukurta.',
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
        'Renginys atnaujintas.',
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
        useLoader(
          `${COMPONENT_LOADING_KEY}_photographers-data`,
          this.loaderService,
        ),
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

  private updateViewPermissions() {
    const groups = this.userService.getUserGroups();
    this.showAssignSelf = groups.includes(UserGroup.Photographer);
    this.showAssignUsers = groups.includes(UserGroup.EventAdmin);
  }

  private setPhotographerList(photographers: EventPhotographer[]) {
    this.eventPhotographers = photographers;
    this.isAssignedSelf = this.userId
      ? photographers.some((u) => this.userId === u.id)
      : false;
  }

  protected readonly SvgIconSrc = SvgIconSrc;
  protected readonly SvgIconSize = SvgIconSize;
}
