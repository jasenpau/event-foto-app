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
import { RouterLink } from '@angular/router';
import { DisposableComponent } from '../../../components/disposable/disposable.component';

@Component({
  selector: 'app-event-list',
  imports: [ButtonComponent, NgForOf, BadgeComponent, NgIf, RouterLink],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent extends DisposableComponent implements OnInit, OnDestroy {

  buttonType = ButtonType;
  buttonIcon = SvgIconSrc;
  badgeType = BadgeType;
  events: EventData[] = [];

  constructor(private eventService: EventService) {
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
  }
}
