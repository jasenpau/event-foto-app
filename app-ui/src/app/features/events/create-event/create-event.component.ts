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
import { of, switchMap, takeUntil, tap } from 'rxjs';
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
import { WatermarkSearchComponent } from '../../watermark/watermark-search/watermark-search.component';
import { WatermarkDisplayComponent } from '../../watermark/watermark-display/watermark-display.component';
import { ModalService } from '../../../services/modal/modal.service';
import { ModalActions } from '../../../services/modal/modal.types';

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
    WatermarkSearchComponent,
    WatermarkDisplayComponent,
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
  protected minEventStartDate = this.getMinDate();
  protected minEventEndDate = this.getMinDate();
  protected createEventForm: FormGroup;
  protected existingNames: string[] = [];
  protected isLoading = false;
  protected selectedWatermarkId?: number | null;
  protected showWatermarkSearch = false;

  constructor(
    private readonly eventService: EventService,
    private readonly snackbarService: SnackbarService,
    private readonly modalService: ModalService,
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
      this.selectedWatermarkId = this.eventToEdit.watermarkId;
    }
  }

  protected toggleWatermarkSearch() {
    this.showWatermarkSearch = !this.showWatermarkSearch;
  }

  protected onWatermarkSelected(watermarkId: number | null) {
    this.selectedWatermarkId = watermarkId;
    this.showWatermarkSearch = false;
  }

  protected removeWatermark() {
    this.selectedWatermarkId = null;
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
        watermarkId: this.selectedWatermarkId,
        reprocessPhotos: false,
      };

      const confirmMessage$ =
        this.eventToEdit &&
        this.eventToEdit.watermarkId !== this.selectedWatermarkId
          ? this.modalService.openConfirmModal({
              body: 'Ar norite apdoroti visas renginio nuotraukas su nauju vandens ženklu?',
              confirm: 'Taip',
              cancel: 'Ne',
            })
          : of(ModalActions.Cancel);

      const request$ = confirmMessage$.pipe(
        switchMap((modalResponse) => {
          eventData.reprocessPhotos = modalResponse === ModalActions.Confirm;
          return this.eventToEdit
            ? this.eventService.updateEvent(this.eventToEdit.id, eventData)
            : this.eventService.createEvent(eventData);
        }),
      );

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

  private getMinDate() {
    const date = new Date();
    date.setFullYear(date.getFullYear() - 1);
    return date;
  }
}
