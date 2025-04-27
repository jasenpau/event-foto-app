import { Component, Input } from '@angular/core';
import { IconButtonComponent } from '../icon-button/icon-button.component';
import { SvgIconSrc } from '../svg-icon/svg-icon.types';
import { LayoutService } from '../../services/layout/layout.service';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-page-header',
  imports: [IconButtonComponent, NgClass],
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.scss',
})
export class PageHeaderComponent {
  @Input() disableStyles = false;

  protected readonly SvgIconSrc = SvgIconSrc;

  constructor(private readonly layoutService: LayoutService) {}

  toggleSidenav() {
    this.layoutService.toggleSideNav();
  }
}
