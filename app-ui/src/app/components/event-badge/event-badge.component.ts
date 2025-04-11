import { Component, Input } from '@angular/core';
import { BadgeComponent } from '../badge/badge.component';
import { BadgeType } from '../badge/badge.types';
import { EventListDto } from '../../services/event/event.types';
import { getStartOfDay } from '../../helpers/getStartOfDay';

@Component({
  selector: 'app-event-badge',
  imports: [BadgeComponent],
  templateUrl: './event-badge.component.html',
})
export class EventBadgeComponent {
  @Input({ required: true }) event!: EventListDto;

  protected readonly badgeType = BadgeType;

  protected isActive(event: EventListDto) {
    const now = new Date();
    const normalizedStartDate = this.normalizeDate(event.startDate);
    const normalizedEndDate = event.endDate
      ? this.addDay(this.normalizeDate(event.endDate))
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

  private normalizeDate(dateString: string) {
    const normalizedDate = new Date(dateString);
    return getStartOfDay(normalizedDate);
  }

  private addDay(date: Date) {
    date.setDate(date.getDate() + 1);
    return date;
  }
}
