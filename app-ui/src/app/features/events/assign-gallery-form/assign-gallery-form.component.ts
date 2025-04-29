import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { NgForOf, NgIf } from '@angular/common';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { useLocalLoader } from '../../../helpers/useLoader';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { SelectComponent } from '../../../components/forms/select/select.component';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';

@Component({
  selector: 'app-assign-gallery-form',
  imports: [
    ButtonComponent,
    NgIf,
    NgForOf,
    ReactiveFormsModule,
    LoaderOverlayComponent,
    SelectComponent,
    FormInputSectionComponent,
  ],
  templateUrl: './assign-gallery-form.component.html',
  styleUrl: './assign-gallery-form.component.scss',
  standalone: true,
})
export class AssignGalleryFormComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  @Input({ required: true }) eventId!: number;
  @Input({ required: true }) photographerId!: string;
  @Input({ required: true }) galleries!: GalleryDto[];
  @Input() preselectedGalleryId?: number;
  @Output() formEvent = new EventEmitter<string>();
  protected readonly ButtonType = ButtonType;

  protected galleryControl = new FormControl<number | null>(null, [
    Validators.required,
  ]);
  protected isLoading = false;

  constructor(private eventService: EventService) {
    super();
  }

  ngOnInit() {
    if (this.preselectedGalleryId) {
      this.galleryControl.patchValue(this.preselectedGalleryId);
    } else {
      const defaultGallery = this.galleries.find((g) => g.isMainGallery);
      if (defaultGallery) {
        this.galleryControl.patchValue(defaultGallery.id);
      }
    }
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected assignGallery() {
    const selectedGalleryId = this.galleryControl.value;
    if (!selectedGalleryId) return;

    this.eventService
      .assignPhotographerToEvent(
        this.eventId,
        selectedGalleryId,
        this.photographerId,
      )
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        tap(() => {
          this.formEvent.emit('assigned');
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
