import { Component, OnDestroy, OnInit } from '@angular/core';
import { takeUntil, tap } from 'rxjs';
import { EventService } from '../../../services/event/event.service';
import { EventListDto } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { BadgeComponent } from '../../../components/badge/badge.component';
import { BadgeType } from '../../../components/badge/badge.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { UserGroup } from '../../../globals/userGroups';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';

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

  events: EventListDto[] = [];
  userGroups: UserGroup[] = [];
  showCreateEvent = false;

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
  }

  ngOnInit() {
    this.getEvents();
    this.getUserPermissions();
  }

  protected formatDate(dateString: string) {
    return formatLithuanianDate(new Date(dateString));
  }

  protected isActive(event: EventListDto) {
    const now = new Date();
    const normalizedStartDate = this.normalizeDate(event.startDate);
    const normalizedEndDate = event.endDate
      ? this.normalizeDate(event.endDate)
      : this.addDay(this.normalizeDate(event.startDate));

    return normalizedStartDate <= now && now <= normalizedEndDate;
  }

  protected isPast(event: EventListDto) {
    const now = new Date();
    const normalizedEndDate = event.endDate
      ? this.normalizeDate(event.endDate)
      : this.addDay(this.normalizeDate(event.startDate));

    return now > normalizedEndDate;
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

  private getUserPermissions() {
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

  private normalizeDate(dateString: string) {
    const normalizedDate = new Date(dateString);
    normalizedDate.setHours(0, 0, 0, 0);
    return normalizedDate;
  }

  private addDay(date: Date) {
    date.setDate(date.getDate() + 1);
    return date;
  }
}
