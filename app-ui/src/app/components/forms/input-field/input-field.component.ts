import { Component, inject, Input } from '@angular/core';
import { NgIf } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormElementBaseDirective } from '../form-element-base/form-element-base.directive';

type SupportedFieldTypes = 'text' | 'password';

@Component({
  selector: 'app-input-field',
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './input-field.component.html',
  styleUrl: './input-field.component.scss',
  hostDirectives: [FormElementBaseDirective],
})
export class InputFieldComponent {
  @Input({ required: true }) id!: string;
  @Input() label: string | undefined = undefined;
  @Input() placeholder: string | undefined = undefined;
  @Input() type: SupportedFieldTypes = 'text';

  hostControl = inject(FormElementBaseDirective);
}
