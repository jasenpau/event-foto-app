import { Component, Input } from '@angular/core';
import { NgClass, NgIf } from '@angular/common';

@Component({
  selector: 'app-form-input-section',
  imports: [NgIf, NgClass],
  templateUrl: './form-input-section.component.html',
  styleUrl: './form-input-section.component.scss',
})
export class FormInputSectionComponent {
  @Input({ required: true }) inputId!: string;
  @Input({ required: true }) label!: string;
  @Input() description?: string;
  @Input() subtext?: string;
  @Input() disableBottomPadding = false;
}
