import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { takeUntil, tap } from 'rxjs';
import { NgForOf, NgIf } from '@angular/common';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { SelectComponent } from '../../../components/forms/select/select.component';
import { useLocalLoader } from '../../../helpers/useLoader';
import { ImageService } from '../../../services/image/image.service';

@Component({
  selector: 'app-move-photos-form',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgIf,
    ButtonComponent,
    LoaderOverlayComponent,
    SelectComponent,
    NgForOf,
  ],
  templateUrl: './move-photos-form.component.html',
  styleUrl: './move-photos-form.component.scss',
})
export class MovePhotosFormComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  @Input({ required: true }) eventId!: number;
  @Input({ required: true }) currentGalleryId!: number;
  @Input({ required: true }) selectedPhotoIds!: Set<number>;

  @Output() formEvent = new EventEmitter<string>();

  protected readonly ButtonType = ButtonType;
  protected form: FormGroup;
  protected isLoading = false;
  protected galleryOptions: GalleryDto[] = [];

  constructor(
    private readonly galleryService: GalleryService,
    private readonly imageService: ImageService,
  ) {
    super();

    this.form = new FormGroup({
      targetGallery: new FormControl<number | null>(null, Validators.required),
    });
  }

  ngOnInit(): void {
    this.galleryService
      .getEventGalleries(this.eventId)
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        takeUntil(this.destroy$),
      )
      .subscribe((galleries) => {
        this.galleryOptions = galleries.filter(
          (g) => g.id !== this.currentGalleryId,
        );

        if (this.galleryOptions.length > 0) {
          this.form.get('targetGallery')?.setValue(this.galleryOptions[0].id);
        }
      });
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    if (!this.form.valid) return;

    const targetId = this.form.value.targetGallery;
    const photoIds = Array.from(this.selectedPhotoIds);

    this.imageService
      .bulkMovePhotos(photoIds, targetId)
      .pipe(
        useLocalLoader((val) => (this.isLoading = val)),
        tap(() => this.formEvent.emit('moved')),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
