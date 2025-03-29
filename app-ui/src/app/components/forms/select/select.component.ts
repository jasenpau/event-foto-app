import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgClass, NgIf } from '@angular/common';
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';

@Component({
  selector: 'app-select',
  imports: [FormsModule, NgIf, ReactiveFormsModule, NgClass],
  templateUrl: './select.component.html',
  styleUrl: './select.component.scss',
})
export class SelectComponent extends FormElementBaseComponent {}
