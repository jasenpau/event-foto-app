import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgClass, NgForOf, NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { EventListDto } from '../../../services/event/event.types';
import { SelectComponent } from '../../../components/forms/select/select.component';
import {
  formatLithuanianDateOnly,
  formatLithuanianTimeOnly,
} from '../../../helpers/formatLithuanianDate';
import { isSameLocalDay } from '../../../helpers/isSameLocalDay';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';

interface CalendarDisplayEvent {
  event: EventListDto;
  startsToday: boolean;
  endsToday: boolean;
}

@Component({
  selector: 'app-event-calendar',
  imports: [
    FormsModule,
    NgForOf,
    SelectComponent,
    ReactiveFormsModule,
    NgIf,
    RouterLink,
    PageHeaderComponent,
    NgClass,
  ],
  templateUrl: './event-calendar.component.html',
  styleUrl: './event-calendar.component.scss',
})
export class EventCalendarComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected calendarForm: FormGroup;
  protected months = [
    'Sausis',
    'Vasaris',
    'Kovas',
    'Balandis',
    'Gegužė',
    'Birželis',
    'Liepa',
    'Rugpjūtis',
    'Rugsėjis',
    'Spalis',
    'Lapkritis',
    'Gruodis',
  ];
  protected years: number[] = [];
  protected weeks: {
    day: number;
    currentMonth: boolean;
    today: boolean;
    events: CalendarDisplayEvent[];
  }[][] = [];
  protected calendarEvents: Record<string, EventListDto[]> = {};

  constructor(private readonly eventService: EventService) {
    super();
    this.calendarForm = new FormGroup({
      year: new FormControl<string>(new Date().getFullYear().toString(), [
        Validators.required,
      ]),
      month: new FormControl<string>(new Date().getMonth().toString(), [
        Validators.required,
      ]),
    });
  }

  ngOnInit() {
    this.populateYears();
    this.getEvents(
      Number(this.calendarForm.value['year']),
      Number(this.calendarForm.value['month']),
    );
    this.calendarForm.valueChanges
      .pipe(
        tap((formValue) => {
          this.getEvents(Number(formValue['year']), Number(formValue['month']));
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  populateYears() {
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 10; i <= currentYear + 10; i++) {
      this.years.push(i);
    }
  }

  generateCalendar(start: Date, end: Date) {
    const today = new Date();
    const month = Number(this.calendarForm.value['month']);

    const current = new Date(start);
    this.weeks = [];
    let weekIndex = -1;

    while (current <= end) {
      if (current.getDay() === 1) {
        weekIndex = weekIndex + 1;
        this.weeks.push([]);
      }

      const events =
        this.calendarEvents[formatLithuanianDateOnly(current)] ?? [];
      events.sort((a, b) => {
        const timeA = a.startDate.split('T')[1];
        const timeB = b.startDate.split('T')[1];
        return timeA.localeCompare(timeB);
      });
      const displayEvents = events.map((event) =>
        this.getEventDisplayData(current, event),
      );

      this.weeks[weekIndex].push({
        day: current.getDate(),
        currentMonth: current.getMonth() === month,
        today: isSameLocalDay(current, today),
        events: displayEvents,
      });
      current.setDate(current.getDate() + 1);
    }
  }

  formatEventTime(dateString: string) {
    return formatLithuanianTimeOnly(new Date(dateString));
  }

  private getCalendarStart(year: number, month: number) {
    const firstOfMonth = new Date(year, month, 1);
    const start = new Date(firstOfMonth);
    const dayOfWeek = start.getDay();
    const diffToMonday = (dayOfWeek === 0 ? -6 : 1) - dayOfWeek;
    start.setDate(start.getDate() + diffToMonday);
    return start;
  }

  private getCalendarEnd(year: number, month: number) {
    const lastOfMonth = new Date(year, month + 1, 0);
    const end = new Date(lastOfMonth);
    const endDayOfWeek = end.getDay();
    const diffToSunday = endDayOfWeek === 0 ? 0 : 7 - endDayOfWeek;
    end.setDate(end.getDate() + diffToSunday);
    return end;
  }

  private getEvents(year: number, month: number) {
    const start = this.getCalendarStart(year, month);
    const end = this.getCalendarEnd(year, month);

    this.eventService
      .searchEvents({
        pageSize: 100,
        fromDate: start,
        toDate: end,
        showArchived: true,
      })
      .pipe(
        tap((events) => {
          this.mapEvents(events.data);
          this.generateCalendar(start, end);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private mapEvents(events: EventListDto[]) {
    this.calendarEvents = {};
    for (const event of events) {
      const eventStartDate = new Date(event.startDate);
      if (event.endDate) {
        const eventEndDate = new Date(event.endDate);
        for (
          let d = eventStartDate;
          d <= eventEndDate;
          d.setDate(d.getDate() + 1)
        ) {
          this.addEventToDay(formatLithuanianDateOnly(d), event);
        }
      } else {
        this.addEventToDay(formatLithuanianDateOnly(eventStartDate), event);
      }
    }
  }

  private addEventToDay(date: string, event: EventListDto) {
    if (this.calendarEvents[date]) {
      this.calendarEvents[date].push(event);
    } else {
      this.calendarEvents[date] = [event];
    }
  }

  private getEventDisplayData(
    currentDate: Date,
    event: EventListDto,
  ): CalendarDisplayEvent {
    const eventStartDate = new Date(event.startDate);
    const eventEndDate = event.endDate ? new Date(event.endDate) : undefined;

    return {
      startsToday: isSameLocalDay(currentDate, eventStartDate),
      endsToday: eventEndDate
        ? isSameLocalDay(currentDate, eventEndDate)
        : true,
      event,
    };
  }
}
