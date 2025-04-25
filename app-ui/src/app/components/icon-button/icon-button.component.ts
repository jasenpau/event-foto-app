import { Component, Input } from '@angular/core';
import { SvgIconSize, SvgIconSrc } from '../svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { NgClass } from '@angular/common';

type IconButtonTheme = 'light' | 'dark';
type IconButtonSize = 'normal' | 'large';

@Component({
  selector: 'app-icon-button',
  imports: [AppSvgIconComponent, NgClass],
  templateUrl: './icon-button.component.html',
  styleUrl: './icon-button.component.scss',
})
export class IconButtonComponent {
  @Input({ required: true }) icon!: SvgIconSrc;
  @Input() theme: IconButtonTheme = 'dark';
  @Input() size: IconButtonSize = 'normal';

  get svgSize() {
    return this.size === 'large' ? SvgIconSize.Large : SvgIconSize.Regular;
  }
}
