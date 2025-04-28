import {
  Component,
  ElementRef,
  EventEmitter,
  OnDestroy,
  Output,
  ViewChild,
} from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { NgIf } from '@angular/common';
import { useLocalLoader } from '../../../helpers/useLoader';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { WatermarkService } from '../../../services/watermark/watermark.service';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';

@Component({
  selector: 'app-watermark-create-form',
  standalone: true,
  imports: [
    ButtonComponent,
    InputFieldComponent,
    NgIf,
    ReactiveFormsModule,
    LoaderOverlayComponent,
    FormInputSectionComponent,
  ],
  templateUrl: './watermark-create-form.component.html',
  styleUrl: './watermark-create-form.component.scss',
})
export class WatermarkCreateFormComponent
  extends DisposableComponent
  implements OnDestroy
{
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @Output() formEvent = new EventEmitter<string>();

  protected readonly ButtonType = ButtonType;
  protected form: FormGroup;
  protected isLoading = false;
  protected previewUrl: string | null = null;
  protected selectedFileName: string | null = null;
  protected fileError: string | undefined;

  constructor(private readonly watermarkService: WatermarkService) {
    super();
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
    });
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFileName = file.name;

      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
      this.fileError = undefined;
    } else {
      this.fileError = 'Prašome pasirinkti nuotraukos failą';
    }
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    const fileList = this.fileInput.nativeElement.files;
    if (!this.form.valid) return;
    if (!fileList || fileList.length === 0) {
      this.fileError = 'Prašome pasirinkti nuotraukos failą';
      return;
    }

    const { name } = this.form.value;
    const file = fileList[0];

    this.watermarkService
      .uploadWatermark(name.trim(), file)
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        tap(() => {
          this.formEvent.emit('created');
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
