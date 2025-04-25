import { Component, Input } from '@angular/core';
import { NgClass, NgIf } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';

type SupportedFieldTypes = 'text' | 'password' | 'email';

@Component({
  selector: 'app-input-field',
  imports: [NgIf, ReactiveFormsModule, NgClass],
  templateUrl: './input-field.component.html',
  styleUrl: './input-field.component.scss',
})
export class InputFieldComponent extends FormElementBaseComponent {
  @Input() type: SupportedFieldTypes = 'text';
  @Input() readonly = false;
}
