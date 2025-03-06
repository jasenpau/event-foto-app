import { Component } from '@angular/core';
import { ReactiveFormsModule } from "@angular/forms";
import { EventService } from '../../../services/event/event.service';
import { NgForOf, NgIf, NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-event-preview',
  imports: [ReactiveFormsModule, NgIf, NgForOf, NgOptimizedImage],
  templateUrl: './event-preview.component.html',
  styleUrl: './event-preview.component.scss',
})
export class EventPreviewComponent {
  protected imageUrls: string[] = [];

  constructor(private readonly eventService: EventService) {
    this.eventService
      .getEventPhotos()
      .subscribe((images) => (this.imageUrls = images));
  }
}
