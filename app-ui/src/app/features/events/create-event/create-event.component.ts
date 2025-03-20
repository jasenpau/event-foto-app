import { Component, OnDestroy } from '@angular/core';
import { ButtonComponent } from "../../../components/button/button.component";
import { Router, RouterLink } from "@angular/router";
import { ButtonType } from '../../../components/button/button.types';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { DatePickerComponent } from '../../../components/forms/date-picker/date-picker.component';
import { TextAreaComponent } from '../../../components/forms/text-area/text-area.component';

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
  implements OnDestroy
{
  protected readonly buttonType = ButtonType;
  protected dateTimeNow = new Date();
  protected createEventForm: FormGroup;

  constructor(
    private readonly eventService: EventService,
    private router: Router,
  ) {
    super();
    this.createEventForm = new FormGroup({
      name: new FormControl('', [Validators.required]),
      startDate: new FormControl('', [Validators.required]),
      endDate: new FormControl(undefined, []),
      location: new FormControl('', []),
      note: new FormControl('', [Validators.maxLength(500)]),
    });
  }

  onSubmit() {
    this.createEventForm.markAllAsTouched();
    console.log(this.createEventForm.value);
    if (false && this.createEventForm.valid) {
      const formData = this.createEventForm.value;
      this.eventService
        .createEvent({ name: formData.name })
        .pipe(
          tap((event) => {
            if (event.id) {
              this.router.navigate(['event']);
            }
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }
}
