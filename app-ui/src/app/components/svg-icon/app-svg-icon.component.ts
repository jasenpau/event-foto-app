import { Component, Input } from '@angular/core';
import { SvgIconSize, SvgIconSrc } from './svg-icon.types';
import { SvgIconComponent } from 'angular-svg-icon';

@Component({
  selector: 'app-svg-icon',
  imports: [
    SvgIconComponent,
  ],
  templateUrl: './app-svg-icon.component.html',
  styleUrl: './app-svg-icon.component.scss',
})
export class AppSvgIconComponent {
  @Input({ required: true }) iconSrc!: SvgIconSrc;
  @Input() size: SvgIconSize = SvgIconSize.Regular;
}
