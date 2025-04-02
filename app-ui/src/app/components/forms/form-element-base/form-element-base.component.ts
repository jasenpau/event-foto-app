import { Component, inject, Input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { FormElementBaseDirective } from './form-element-base.directive';

@Component({
  selector: 'app-form-element-base',
  imports: [ReactiveFormsModule],
  template: '',
  hostDirectives: [FormElementBaseDirective],
})
export class FormElementBaseComponent {
  @Input({ required: true }) id!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() supportingText?: string;

  hostControl = inject(FormElementBaseDirective);

  getError(): string | null {
    const errors = this.hostControl.control.errors;
    if (!errors) return null;

    if (errors['required']) return 'Laukelis privalomas';
    if (errors['email']) return 'Netinkamas el. pašto adresas';
    if (errors['maxlength'])
      return `Įvestis turi būti iki ${errors['maxlength'].requiredLength} simbolių`;
    if (errors['duplicate']) return 'Ši reikšmė jau naudojama';
    return 'Klaida';
  }

  showErrors = () =>
    Boolean(
      this.hostControl.control.touched &&
        this.hostControl.control.invalid &&
        this.hostControl.control.errors,
    );
}
