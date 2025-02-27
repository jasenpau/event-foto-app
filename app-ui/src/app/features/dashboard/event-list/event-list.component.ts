import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil, tap } from 'rxjs';
import { EventService } from '../../../services/event/event.service';
import { EventData } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/actions/button/button.component';
import { ButtonType } from '../../../components/actions/button/button.types';
import { NgForOf } from '@angular/common';

@Component({
  selector: 'app-event-list',
  imports: [ButtonComponent, NgForOf],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent implements OnInit, OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();

  buttonType = ButtonType;
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
