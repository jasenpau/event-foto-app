import { Component, OnDestroy, OnInit } from '@angular/core';
import { takeUntil, tap } from 'rxjs';
import { EventService } from '../../../services/event/event.service';
import { EventData } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { BadgeComponent } from '../../../components/badge/badge.component';
import { BadgeType } from '../../../components/badge/badge.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { UserGroup } from '../../../globals/userGroups';

@Component({
  selector: 'app-event-list',
  imports: [ButtonComponent, NgForOf, BadgeComponent, NgIf],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  buttonType = ButtonType;
  buttonIcon = SvgIconSrc;
  badgeType = BadgeType;

  events: EventData[] = [];
  userGroups: UserGroup[] = [];
  showCreateEvent = false;

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
  }

  ngOnInit() {
    this.eventService
      .getEvents()
      .pipe(
        tap((events: EventData[]) => {
          this.events = events;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();

    this.userService
      .userGroups()
      .pipe(
        tap((groups) => {
          this.userGroups = groups;
          this.updateViewPermissions();
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private updateViewPermissions() {
    this.showCreateEvent = this.userGroups.includes(UserGroup.EventAdmin);
  }
}
