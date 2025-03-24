import { Component, OnDestroy, OnInit } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { Router, RouterLink } from '@angular/router';
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
import { noDuplicatesValidator } from '../../../components/forms/validators/noDuplicatesValidator';

@Component({
  selector: 'app-create-event',
  imports: [
    ButtonComponent,
    RouterLink,
    FormInputSectionComponent,
    InputFieldComponent,
    FormsModule,
    ReactiveFormsModule,
    DatePickerComponent,
    TextAreaComponent,
  ],
  templateUrl: './create-event.component.html',
  styleUrl: './create-event.component.scss',
})
export class CreateEventComponent
  extends DisposableComponent
  implements OnDestroy, OnInit
{
  protected readonly buttonType = ButtonType;
  protected minEventStartDate = new Date();
  protected minEventEndDate = new Date();
  protected createEventForm: FormGroup;
  protected existingNames: string[] = [];

  constructor(
    private readonly eventService: EventService,
    private router: Router,
  ) {
    super();
    this.createEventForm = new FormGroup({
      name: new FormControl('', [
        Validators.required,
        noDuplicatesValidator(
          this.existingNames,
          'Renginys tokiu pavadinimu jau įvestas sistemoje',
        ),
      ]),
      startDate: new FormControl('', [Validators.required]),
      endDate: new FormControl(undefined, []),
      location: new FormControl('', []),
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
  }

  onSubmit() {
    this.createEventForm.markAllAsTouched();
    if (this.createEventForm.valid) {
      const formData = this.createEventForm.value;
      this.eventService
        .createEvent({
          name: formData.name,
          startDate: formData.startDate,
          endDate: formData.endDate,
          location: formData.location,
          note: formData.note,
        })
        .pipe(
          tap((event) => {
            if (event.id) {
              this.router.navigate(['event']);
            }
          }),
          handleApiError((error) => {
            console.log(error);
            if (error.status === 409) {
              this.addConflictingName(formData.name);
            }
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  private addConflictingName(name: string) {
    this.existingNames.push(name.trim());
    this.createEventForm.controls['name'].updateValueAndValidity();
  }
}
