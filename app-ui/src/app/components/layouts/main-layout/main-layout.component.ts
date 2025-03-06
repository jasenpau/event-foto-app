import { Component } from '@angular/core';
import { SidenavComponent } from '../../sidenav/sidenav.component';
import { HeaderComponent } from '../../header/header.component';

@Component({
  selector: 'app-main-layout',
  imports: [SidenavComponent, HeaderComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {}
