import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { Router } from '@angular/router';
import { ButtonType } from '../../../components/button/button.types';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { DatePickerComponent } from '../../../components/forms/date-picker/date-picker.component';
import { TextAreaComponent } from '../../../components/forms/text-area/text-area.component';
import { handleApiError } from '../../../helpers/handleApiError';
import { invalidValues } from '../../../components/forms/validators/invalidValues';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { NgIf } from '@angular/common';
import { useLocalLoader } from '../../../helpers/useLoader';
import { EventDto } from '../../../services/event/event.types';

@Component({
  selector: 'app-create-event',
  imports: [
    ButtonComponent,
    FormInputSectionComponent,
    InputFieldComponent,
    FormsModule,
    ReactiveFormsModule,
    DatePickerComponent,
    TextAreaComponent,
    LoaderOverlayComponent,
    NgIf,
  ],
  templateUrl: './create-event.component.html',
  styleUrl: './create-event.component.scss',
})
export class CreateEventComponent
  extends DisposableComponent
  implements OnDestroy, OnInit
{
  @Input() eventToEdit?: EventDto;
  @Output() formEvent = new EventEmitter<string>();

  protected readonly buttonType = ButtonType;
  protected minEventStartDate = new Date();
  protected minEventEndDate = new Date();
  protected createEventForm: FormGroup;
  protected existingNames: string[] = [];
  protected isLoading = false;

  constructor(
    private readonly eventService: EventService,
    private readonly snackbarService: SnackbarService,
    private router: Router,
  ) {
    super();
    this.createEventForm = new FormGroup({
      name: new FormControl('', [
        Validators.required,
        Validators.maxLength(255),
        invalidValues(
          this.existingNames,
          'Renginys tokiu pavadinimu jau įvestas sistemoje',
        ),
      ]),
      startDate: new FormControl('', [Validators.required]),
      endDate: new FormControl(undefined, []),
      location: new FormControl('', [Validators.maxLength(255)]),
      note: new FormControl('', [Validators.maxLength(500)]),
    });
  }

  ngOnInit() {
    this.createEventForm.controls['startDate'].valueChanges
      .pipe(
        tap((value) => {
          if (value instanceof Date) {
            this.minEventEndDate = value;
            const endDate = this.createEventForm.value['endDate'];
            if (endDate instanceof Date && value > endDate) {
              this.createEventForm.patchValue({ endDate: null });
            }
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();

    if (this.eventToEdit) {
      this.createEventForm.patchValue({
        name: this.eventToEdit.name,
        startDate: new Date(this.eventToEdit.startDate),
        endDate: this.eventToEdit.endDate
          ? new Date(this.eventToEdit.endDate)
          : undefined,
        location: this.eventToEdit.location || '',
        note: this.eventToEdit.note || '',
      });
    }
  }

  onSubmit() {
    this.createEventForm.markAllAsTouched();
    if (this.createEventForm.valid) {
      const formData = this.createEventForm.value;
      const eventData = {
        name: formData.name,
        startDate: formData.startDate,
        endDate: formData.endDate,
        location: formData.location,
        note: formData.note,
      };

      const request$ = this.eventToEdit
        ? this.eventService.updateEvent(this.eventToEdit.id, eventData)
        : this.eventService.createEvent(eventData);

      request$
        .pipe(
          useLocalLoader((value) => (this.isLoading = value)),
          tap((event) => {
            if (event.id) {
              if (!this.eventToEdit) {
                this.snackbarService.addSnackbar(
                  SnackbarType.Success,
                  'Renginys sukurtas',
                );
                this.router.navigate(['/event', event.id]);
              } else {
                this.formEvent.emit('updated');
              }
            }
          }),
          handleApiError((error) => {
            if (error.status === 409) {
              this.addConflictingName(formData.name);
            }
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  private addConflictingName(name: string) {
    this.existingNames.push(name.trim());
    this.createEventForm.controls['name'].updateValueAndValidity();
  }
}
