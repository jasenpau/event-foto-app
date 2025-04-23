import { Component } from '@angular/core';
import { SpinnerComponent } from '../spinner/spinner.component';

@Component({
  selector: 'app-loader-overlay',
  imports: [SpinnerComponent],
  templateUrl: './loader-overlay.component.html',
  styleUrl: './loader-overlay.component.scss',
})
export class LoaderOverlayComponent {}
