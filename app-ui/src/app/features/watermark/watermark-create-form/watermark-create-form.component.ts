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

@Component({
  selector: 'app-watermark-create-form',
  standalone: true,
  imports: [
    ButtonComponent,
    InputFieldComponent,
    NgIf,
    ReactiveFormsModule,
    LoaderOverlayComponent,
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

  constructor(private readonly watermarkService: WatermarkService) {
    super();
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
      file: new FormControl(null, [Validators.required]),
    });
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected onSubmit() {
    this.form.markAllAsTouched();
    const fileList = this.fileInput.nativeElement.files;
    if (!this.form.valid || !fileList) return;

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
