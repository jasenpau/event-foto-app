import { Component, Input } from '@angular/core';
import {
  UploadEventType,
  UploadMessage,
} from '../../features/camera/camera.types';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-upload-tracker',
  imports: [NgIf],
  templateUrl: './upload-tracker.component.html',
  styleUrl: './upload-tracker.component.scss',
})
export class UploadTrackerComponent {
  activeUploads = new Set<string>();
  erroredUploads = new Set<string>();
  totalUploads = 0;
  successfulUploads = 0;

  @Input() set uploadEvent(message: UploadMessage | undefined) {
    if (!message) return;

    switch (message.eventType) {
      case UploadEventType.UploadStart:
        this.activeUploads.add(message.filename);
        this.totalUploads += 1;
        break;
      case UploadEventType.UploadComplete:
        this.activeUploads.delete(message.filename);
        this.successfulUploads += 1;
        break;
      case UploadEventType.UploadError:
        this.activeUploads.delete(message.filename);
        this.erroredUploads.add(message.filename);
        break;
    }
  }

  get inProgress(): number {
    return this.activeUploads.size;
  }
}
