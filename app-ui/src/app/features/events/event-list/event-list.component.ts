import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil, tap } from 'rxjs';
import { EventService } from '../../../services/event/event.service';
import { EventData } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { BadgeComponent } from '../../../components/badge/badge.component';
import { BadgeType } from '../../../components/badge/badge.types';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-event-list',
  imports: [ButtonComponent, NgForOf, BadgeComponent, NgIf, RouterLink],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent implements OnInit, OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();

  buttonType = ButtonType;
  buttonIcon = SvgIconSrc;
  badgeType = BadgeType;
  events: EventData[] = [];

  constructor(private eventService: EventService) {}

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
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
