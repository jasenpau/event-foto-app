import { Component } from '@angular/core';
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgClass, NgIf } from '@angular/common';

@Component({
  selector: 'app-text-area',
  imports: [FormsModule, NgIf, ReactiveFormsModule, NgClass],
  templateUrl: './text-area.component.html',
  styleUrl: './text-area.component.scss',
})
export class TextAreaComponent extends FormElementBaseComponent {}
