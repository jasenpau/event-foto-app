import { Component } from '@angular/core';
import { SidenavComponent } from '../../sidenav/sidenav.component';

@Component({
  selector: 'app-main-layout',
  imports: [SidenavComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {}
