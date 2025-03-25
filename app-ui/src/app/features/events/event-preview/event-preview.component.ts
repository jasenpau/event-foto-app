import { Component, OnDestroy } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import { EventDto } from '../../../services/event/event.types';
import { NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntil, tap } from 'rxjs';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { BadgeComponent } from '../../../components/badge/badge.component';
import { BadgeType } from '../../../components/badge/badge.types';

@Component({
  selector: 'app-event-preview',
  imports: [
    ReactiveFormsModule,
    NgIf,
    RouterLink,
    ButtonComponent,
    BadgeComponent,
  ],
  templateUrl: './event-preview.component.html',
  styleUrl: './event-preview.component.scss',
})
export class EventPreviewComponent
  extends DisposableComponent
  implements OnDestroy
{
  protected readonly ButtonType = ButtonType;
  protected readonly BadgeType = BadgeType;

  protected event?: EventDto;

  constructor(
    private readonly eventService: EventService,
    private route: ActivatedRoute,
  ) {
    super();
    this.readRouteParams();
  }

  protected formatDate(dateString: string): string {
    return formatLithuanianDate(new Date(dateString));
  }

  protected removePhotographer() {
    console.log('Removing photographer...');
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('eventId'));
      if (!isNaN(id) && id > 0) {
        this.loadEvent(id);
      }
    });
  }

  private loadEvent(id: number) {
    this.eventService
      .getEventDetails(id)
      .pipe(
        tap((event) => {
          this.event = event;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
