import { Component, OnDestroy, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import {
  EventDto,
  EventPhotographer,
} from '../../../services/event/event.types';
import { NgForOf, NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntil, tap } from 'rxjs';
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

const COMPONENT_LOADER_KEY = 'event-preview';

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
  protected userId?: string;

  constructor(
    private readonly eventService: EventService,
    private readonly userService: UserService,
    private readonly route: ActivatedRoute,
    private readonly snackbarService: SnackbarService,
    private readonly loaderService: LoaderService,
  ) {
    super();
    this.loaderService.startLoading(COMPONENT_LOADER_KEY);
    this.readRouteParams();
  }

  ngOnInit() {
    this.userId = this.userService.getCurrentUserData()?.id;
    this.userService.userGroupsCallback((groups) => {
      this.updateViewPermissions(groups);
    });
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

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('eventId'));
      if (!isNaN(id) && id > 0) {
        this.loadEvent(id);
        this.loadPhotographers(id);
      }
    });
  }

  private loadEvent(id: number) {
    this.eventService
      .getEventDetails(id)
      .pipe(
        tap((event) => {
          this.event = event;
          this.loaderService.finishLoading(COMPONENT_LOADER_KEY);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private loadPhotographers(eventId: number) {
    this.eventService
      .getEventPhotographers(eventId)
      .pipe(
        tap((data) => {
          this.setPhotographerList(data);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private updateViewPermissions(groups: UserGroup[]) {
    this.showAssignSelf = groups.includes(UserGroup.Photographer);
    this.showAssignUsers = groups.includes(UserGroup.EventAdmin);
  }

  private setPhotographerList(photographers: EventPhotographer[]) {
    this.eventPhotographers = photographers;
    this.isAssignedSelf = this.userId
      ? photographers.some((u) => this.userId === u.id)
      : false;
  }
}
