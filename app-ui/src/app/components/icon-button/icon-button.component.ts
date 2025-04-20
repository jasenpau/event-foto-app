import { Component, Input } from '@angular/core';
import { SvgIconSrc } from '../svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { NgClass } from '@angular/common';

type IconButtonTheme = 'light' | 'dark';

@Component({
  selector: 'app-icon-button',
  imports: [AppSvgIconComponent, NgClass],
  templateUrl: './icon-button.component.html',
  styleUrl: './icon-button.component.scss',
})
export class IconButtonComponent {
  @Input({ required: true }) icon!: SvgIconSrc;
  @Input() theme: IconButtonTheme = 'dark';
}
