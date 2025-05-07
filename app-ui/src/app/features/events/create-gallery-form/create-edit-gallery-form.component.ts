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
import { of, switchMap, takeUntil, tap } from 'rxjs';
import { handleApiError } from '../../../helpers/handleApiError';
import { NgIf } from '@angular/common';
import { useLocalLoader } from '../../../helpers/useLoader';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { GalleryService } from '../../../services/gallery/gallery.service';
import { GalleryDto } from '../../../services/gallery/gallery.types';
import { WatermarkSearchComponent } from '../../watermark/watermark-search/watermark-search.component';
import { WatermarkDisplayComponent } from '../../watermark/watermark-display/watermark-display.component';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';
import { ModalActions } from '../../../services/modal/modal.types';
import { ModalService } from '../../../services/modal/modal.service';

@Component({
  selector: 'app-create-gallery-form',
  imports: [
    ButtonComponent,
    InputFieldComponent,
    FormsModule,
    NgIf,
    ReactiveFormsModule,
    LoaderOverlayComponent,
    WatermarkSearchComponent,
    WatermarkDisplayComponent,
    FormInputSectionComponent,
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
  protected selectedWatermarkId: number | null = null;
  protected showWatermarkSearch = false;

  private existingNames: string[] = [];

  constructor(
    private readonly galleryService: GalleryService,
    private readonly modalService: ModalService,
  ) {
    super();
    this.form = new FormGroup({
      name: new FormControl<string>('', [
        Validators.required,
        invalidValues(
          this.existingNames,
          'Galerija tokiu pavadinimu šiame renginyje jau yra',
        ),
      ]),
    });
  }

  ngOnInit() {
    if (this.galleryToEdit) {
      this.form.patchValue({
        name: this.galleryToEdit.name,
      });
      this.selectedWatermarkId = this.galleryToEdit.watermarkId;
    }
  }

  protected toggleWatermarkSearch() {
    this.showWatermarkSearch = !this.showWatermarkSearch;
  }

  protected onWatermarkSelected(watermarkId: number | null) {
    this.selectedWatermarkId = watermarkId;
    this.showWatermarkSearch = false;
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    if (!this.form.valid) return;

    const galleryData = {
      name: this.form.value.name.trim(),
      watermarkId: this.selectedWatermarkId,
      reprocessPhotos: false,
    };

    const confirmMessage$ =
      this.galleryToEdit &&
      this.galleryToEdit.watermarkId !== this.selectedWatermarkId
        ? this.modalService.openConfirmModal({
            body: 'Ar norite apdoroti visas galerijos nuotraukas su nauju vandens ženklu?',
            confirm: 'Taip',
            cancel: 'Ne',
          })
        : of(ModalActions.Cancel);

    const request$ = confirmMessage$.pipe(
      switchMap((modalResponse) => {
        galleryData.reprocessPhotos = modalResponse === ModalActions.Confirm;
        return this.galleryToEdit
          ? this.galleryService.updateGallery(
              this.galleryToEdit.id,
              galleryData,
            )
          : this.galleryService.createEventGallery(this.eventId, galleryData);
      }),
    );

    request$
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        tap(() => {
          this.formEvent.emit(this.galleryToEdit ? 'updated' : 'created');
        }),
        handleApiError((error) => {
          if (error.status === 409) {
            this.addConflictingName(galleryData.name);
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

  removeWatermark() {
    this.selectedWatermarkId = null;
  }
}
