import { Component } from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PaginationControlsComponent } from '../../../components/pagination-controls/pagination-controls.component';

@Component({
  selector: 'app-watermark-search',
  imports: [
    InputFieldComponent,
    ReactiveFormsModule,
    PaginationControlsComponent,
  ],
  templateUrl: './watermark-search.component.html',
  styleUrl: './watermark-search.component.scss',
})
export class WatermarkSearchComponent {}
