import { Component, OnDestroy, OnInit } from '@angular/core';
import { takeUntil, tap } from 'rxjs';
import { EventService } from '../../../services/event/event.service';
import { EventListDto } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { UserGroup } from '../../../globals/userGroups';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { EventBadgeComponent } from '../../../components/event-badge/event-badge.component';

@Component({
  selector: 'app-event-list',
  imports: [ButtonComponent, NgForOf, NgIf, EventBadgeComponent],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  buttonType = ButtonType;
  buttonIcon = SvgIconSrc;

  events: EventListDto[] = [];
  showCreateEvent = false;

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
  }

  ngOnInit() {
    this.getEvents();
    this.userService.userGroupsCallback((groups) => {
      this.updateViewPermissions(groups);
    });
  }

  protected formatDate(dateString: string) {
    return formatLithuanianDate(new Date(dateString));
  }

  private getEvents() {
    this.eventService
      .getEvents()
      .pipe(
        tap((events: EventListDto[]) => {
          this.events = events;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private updateViewPermissions(groups: UserGroup[]) {
    this.showCreateEvent = groups.includes(UserGroup.EventAdmin);
  }
}
