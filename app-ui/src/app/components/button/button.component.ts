import { Component, Input } from '@angular/core';
import { ButtonSize, ButtonType } from './button.types';
import { NgClass, NgIf } from '@angular/common';
import { SvgIconSize, SvgIconSrc } from '../svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';

@Component({
  selector: 'app-button',
  imports: [NgClass, AppSvgIconComponent, NgIf],
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
})
export class ButtonComponent {
  @Input() type: ButtonType = ButtonType.Filled;
  @Input() size: ButtonSize = ButtonSize.Regular;
  @Input() icon: SvgIconSrc | null = null;

  buttonType = ButtonType;
  buttonSize = ButtonSize;
  iconSize = SvgIconSize.Small;
}
