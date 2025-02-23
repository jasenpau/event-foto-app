import { Component, Input } from '@angular/core';
import { ButtonType } from './button.types';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-button',
  imports: [NgClass],
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
})
export class ButtonComponent {
  @Input() type: ButtonType = ButtonType.Regular;

  buttonType = ButtonType;
}
