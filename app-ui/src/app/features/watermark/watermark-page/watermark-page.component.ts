import { Component } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { NgIf } from '@angular/common';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { WatermarkSearchComponent } from '../watermark-search/watermark-search.component';

@Component({
  selector: 'app-watermark-page',
  imports: [
    ButtonComponent,
    NgIf,
    PageHeaderComponent,
    WatermarkSearchComponent,
  ],
  templateUrl: './watermark-page.component.html',
  styleUrl: './watermark-page.component.scss',
})
export class WatermarkPageComponent {
  protected readonly SvgIconSrc = SvgIconSrc;

  protected showCreateForm = false;

  protected openCreateForm() {
    this.showCreateForm = true;
  }
}
