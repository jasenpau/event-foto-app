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
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { invalidValues } from '../../../components/forms/validators/invalidValues';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { handleApiError } from '../../../helpers/handleApiError';
import { NgIf } from '@angular/common';
import { useLocalLoader } from '../../../helpers/useLoader';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';

@Component({
  selector: 'app-create-gallery-form',
  imports: [
    ButtonComponent,
    InputFieldComponent,
    FormsModule,
    NgIf,
    ReactiveFormsModule,
    LoaderOverlayComponent,
  ],
  templateUrl: './create-edit-gallery-form.component.html',
  styleUrl: './create-edit-gallery-form.component.scss',
})
export class CreateEditGalleryFormComponent
  extends DisposableComponent
  implements OnDestroy, OnInit
{
  @Input({ required: true }) eventId!: number;
  @Input() galleryToEdit?: GalleryDto;
  @Output() formEvent = new EventEmitter<string>();

  protected readonly ButtonType = ButtonType;
  protected form: FormGroup;
  protected isLoading = false;

  private existingNames: string[] = [];

  constructor(private readonly galleryService: GalleryService) {
    super();
    this.form = new FormGroup({
      name: new FormControl('', [
        Validators.required,
        invalidValues(
          this.existingNames,
          'Galerija tokiu pavadinimu šiame renginyje jau yra.',
        ),
      ]),
    });
  }

  ngOnInit() {
    if (this.galleryToEdit) {
      this.form.patchValue({
        name: this.galleryToEdit.name,
      });

      this.existingNames.push(this.galleryToEdit.name);
    }
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    if (!this.form.valid) return;

    const galleryName = this.form.value.name.trim();

    const request$ = this.galleryToEdit
      ? this.galleryService.updateGallery(this.galleryToEdit.id, galleryName)
      : this.galleryService.createEventGallery(this.eventId, galleryName);

    request$
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        tap(() => {
          this.formEvent.emit(this.galleryToEdit ? 'updated' : 'created');
        }),
        handleApiError((error) => {
          if (error.status === 409) {
            this.addConflictingName(galleryName);
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private addConflictingName(name: string) {
    this.existingNames.push(name.trim());
    this.form.controls['name'].updateValueAndValidity();
  }
}
