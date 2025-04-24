import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  ViewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ImageService } from '../../../services/image/image.service';
import { ButtonType } from '../../../components/button/button.types';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../../../components/svg-icon/app-svg-icon.component';

@Component({
  selector: 'app-upload-photo-form',
  imports: [FormsModule, AppSvgIconComponent],
  templateUrl: './upload-photo-form.component.html',
  styleUrl: './upload-photo-form.component.scss',
})
export class UploadPhotoFormComponent
  extends DisposableComponent
  implements OnDestroy
{
  @Input({ required: true }) eventId!: number;
  @Input({ required: true }) galleryId!: number;
  @Output() photoUploadStarted = new EventEmitter<string>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  selectedFiles: File[] = [];

  constructor(private readonly imageService: ImageService) {
    super();
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFiles = input.files ? Array.from(input.files) : [];
    this.upload();
  }

  upload(): void {
    if (!this.selectedFiles.length) return;

    this.imageService.uploadPhotos(
      this.selectedFiles,
      this.eventId,
      this.galleryId,
    );
    this.photoUploadStarted.emit('started');
  }

  protected readonly ButtonType = ButtonType;
  protected readonly SvgIconSrc = SvgIconSrc;
}
