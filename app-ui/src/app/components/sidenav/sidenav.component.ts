import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgForOf } from '@angular/common';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { SvgIconSrc } from '../svg-icon/svg-icon.types';
import { User } from '../../services/auth/auth.types';
import { AuthService } from '../../services/auth/auth.service';

interface SidenavItem {
  title: string;
  link: string;
  icon: SvgIconSrc;
}

@Component({
  selector: 'app-sidenav',
  imports: [RouterLink, RouterLinkActive, NgForOf, AppSvgIconComponent],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.scss',
})
export class SidenavComponent {
  svgIconSrc = SvgIconSrc;
  sidenavItems: SidenavItem[] = [
    { title: 'Renginiai', link: '/event', icon: SvgIconSrc.Ticket },
    { title: 'Kalendorius', link: '/calendar', icon: SvgIconSrc.Calendar },
    { title: 'Naudotojai', link: '/users', icon: SvgIconSrc.Users },
  ];

  currentUser: User | null = null;

  constructor(private authService: AuthService) {
    this.currentUser = this.authService.getUserTokenData();
  }

  logout() {
    this.authService.msalLogout();
  }
}
