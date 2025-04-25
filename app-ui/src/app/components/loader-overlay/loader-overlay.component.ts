import { Component, Input } from '@angular/core';
import { SpinnerComponent } from '../spinner/spinner.component';
import { NgClass } from '@angular/common';

type OverlayTheme = 'white' | 'transparent';

@Component({
  selector: 'app-loader-overlay',
  imports: [SpinnerComponent, NgClass],
  templateUrl: './loader-overlay.component.html',
  styleUrl: './loader-overlay.component.scss',
})
export class LoaderOverlayComponent {
  @Input() theme: OverlayTheme = 'white';
}
