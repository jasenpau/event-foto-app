import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
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
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { handleApiError } from '../../../helpers/handleApiError';
import { NgIf } from '@angular/common';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { useLocalLoader } from '../../../helpers/useLoader';

@Component({
  selector: 'app-create-gallery-form',
  imports: [
    ButtonComponent,
    InputFieldComponent,
    FormsModule,
    NgIf,
    SpinnerComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './create-gallery-form.component.html',
  styleUrl: './create-gallery-form.component.scss',
})
export class CreateGalleryFormComponent
  extends DisposableComponent
  implements OnDestroy
{
  @Input({ required: true }) eventId!: number;
  @Output() formEvent = new EventEmitter<string>();

  protected readonly ButtonType = ButtonType;
  protected form: FormGroup;
  protected isLoading = false;

  private existingNames: string[] = [];

  constructor(private readonly eventService: EventService) {
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

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    if (this.form.valid) {
      const galleryName = this.form.value.name;
      this.eventService
        .createEventGallery(this.eventId, galleryName)
        .pipe(
          useLocalLoader((value) => (this.isLoading = value)),
          tap(() => {
            this.formEvent.emit('created');
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
  }

  private addConflictingName(name: string) {
    this.existingNames.push(name.trim());
    this.form.controls['name'].updateValueAndValidity();
  }
}
