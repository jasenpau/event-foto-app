import { Component, inject, Input } from '@angular/core';
import { NgClass, NgIf } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormElementBaseDirective } from '../form-element-base/form-element-base.directive';

type SupportedFieldTypes = 'text' | 'password';

@Component({
  selector: 'app-input-field',
  imports: [NgIf, ReactiveFormsModule, NgClass],
  templateUrl: './input-field.component.html',
  styleUrl: './input-field.component.scss',
  hostDirectives: [FormElementBaseDirective],
})
export class InputFieldComponent {
  @Input({ required: true }) id!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() supportingText?: string;
  @Input() type: SupportedFieldTypes = 'text';

  hostControl = inject(FormElementBaseDirective);

  getError(): string | null {
    const errors = this.hostControl.control.errors;
    if (!errors) return null;

    if (errors['required']) return 'Laukelis privalomas';
    return 'Klaida';
  }

  showErrors = () => Boolean(this.hostControl.control.touched &&
    this.hostControl.control.invalid &&
    this.hostControl.control.errors);
}
