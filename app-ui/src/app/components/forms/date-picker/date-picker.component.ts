import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgClass, NgIf } from '@angular/common';
import { FormElementBaseComponent } from '../form-element-base/form-element-base.component';
import {
  FlatpickrDirective,
  provideFlatpickrDefaults,
} from 'angularx-flatpickr';
import flatpickr from 'flatpickr';
import confirmDatePlugin from 'flatpickr/dist/plugins/confirmDate/confirmDate';
import {
  formatLithuanianDate,
  formatLithuanianDateOnly,
} from '../../../helpers/formatLithuanianDate';

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
  @Input() dateOnly = false;
  @Input({ required: false }) minDate!: Date;
  @Input() subtext?: string;

  protected plugins = [confirmDatePlugin({ confirmText: 'Patvirtinti' })];

  // @ts-expect-error Types in angularx-flatpickr library are incorrect, this is a workaround
  formatLtDate(...args) {
    const value = args[0];
    const format = args[1] ?? undefined;
    if (format === 'lt-date-only') return formatLithuanianDateOnly(value);
    if (format === 'lt-format') return formatLithuanianDate(value);
    return flatpickr.formatDate(value, format);
  }

  get altFormat(): string {
    return this.dateOnly ? 'lt-date-only' : 'lt-format';
  }
}
