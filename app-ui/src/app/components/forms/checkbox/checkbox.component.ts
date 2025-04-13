import { Component } from '@angular/core';
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';
import { NgClass, NgIf } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-checkbox',
  imports: [NgClass, ReactiveFormsModule, NgIf],
  templateUrl: './checkbox.component.html',
  styleUrl: './checkbox.component.scss',
})
export class CheckboxComponent extends FormElementBaseComponent {}
