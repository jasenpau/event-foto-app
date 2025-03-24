import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgClass, NgIf } from "@angular/common";
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';
import { FlatpickrDirective, provideFlatpickrDefaults } from 'angularx-flatpickr';
import flatpickr from 'flatpickr';
import confirmDatePlugin from 'flatpickr/dist/plugins/confirmDate/confirmDate';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';

@Component({
  selector: 'app-date-picker',
  imports: [
    FormsModule,
    NgIf,
    NgClass,
    ReactiveFormsModule,
    FlatpickrDirective,
  ],
  providers: [provideFlatpickrDefaults()],
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.scss',
})
export class DatePickerComponent extends FormElementBaseComponent {
  @Input() enableTime = false;
  @Input({ required: false }) minDate!: Date;
  @Input() subtext?: string;

  protected plugins = [confirmDatePlugin({ confirmText: 'Patvirtinti' })];

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  formatLtDate(value: any, format: any) {
    if (format === 'lt-format') return formatLithuanianDate(value);
    return flatpickr.formatDate(value, format);
  }
}
