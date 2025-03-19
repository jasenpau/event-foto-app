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

@Component({
  selector: 'app-create-event',
  imports: [
    ButtonComponent,
    RouterLink,
    FormInputSectionComponent,
    InputFieldComponent,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: './create-event.component.html',
  styleUrl: './create-event.component.scss',
})
export class CreateEventComponent extends DisposableComponent implements OnDestroy {

  protected readonly buttonType = ButtonType;
  protected createEventForm: FormGroup;

  constructor(private readonly eventService: EventService,
              private router: Router) {
    super();
    this.createEventForm = new FormGroup({
      name: new FormControl('', [Validators.required]),
    });
  }

  onSubmit() {
    this.createEventForm.markAllAsTouched();
    if (this.createEventForm.valid) {
      const formData = this.createEventForm.value;
      this.eventService.createEvent({ name: formData.name })
        .pipe(
          tap((event) => {
            if (event.id) {
              this.router.navigate(['event']);
            }
          }),
          takeUntil(this.destroy$)
        )
        .subscribe();
    }
  }
}
