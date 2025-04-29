import { Component, Input } from '@angular/core';
import { ButtonSize, ButtonType } from './button.types';
import { NgClass, NgIf, NgTemplateOutlet } from '@angular/common';
import { SvgIconSize, SvgIconSrc } from '../svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-button',
  imports: [NgClass, AppSvgIconComponent, NgIf, RouterLink, NgTemplateOutlet],
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
})
export class ButtonComponent {
  @Input() type: ButtonType = ButtonType.Filled;
  @Input() size: ButtonSize = ButtonSize.Regular;
  @Input() icon?: SvgIconSrc;
  @Input() link?: string;
  @Input() disabled?: boolean;
  @Input() disableFormSubmit?: boolean = false;
  @Input() download?: string;

  protected readonly buttonType = ButtonType;
  protected readonly buttonSize = ButtonSize;
  protected readonly iconSize = SvgIconSize.Small;
}
