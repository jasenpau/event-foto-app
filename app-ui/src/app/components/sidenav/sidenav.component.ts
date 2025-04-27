import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgForOf } from '@angular/common';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { SvgIconSrc } from '../svg-icon/svg-icon.types';
import { User } from '../../services/auth/auth.types';
import { AuthService } from '../../services/auth/auth.service';
import { UserService } from '../../services/user/user.service';
import { UserGroup } from '../../globals/userGroups';

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
  sidenavItems: SidenavItem[];

  currentUser: User | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
  ) {
    this.currentUser = this.authService.getUserTokenData();
    this.sidenavItems = this.generateSidenavItems();
  }

  logout() {
    this.authService.msalLogout();
  }

  private generateSidenavItems(): SidenavItem[] {
    const groups = this.userService.getUserGroups();
    const sidenavItems: SidenavItem[] = [
      { title: 'Renginiai', link: '/event', icon: SvgIconSrc.Ticket },
      { title: 'Kalendorius', link: '/calendar', icon: SvgIconSrc.Calendar },
    ];

    if (groups.includes(UserGroup.Photographer)) {
      sidenavItems.push({
        title: 'Fotoaparatas',
        link: '/camera',
        icon: SvgIconSrc.Camera,
      });
    }

    if (groups.includes(UserGroup.EventAdmin)) {
      sidenavItems.push({
        title: 'Vandens ženklai',
        link: '/watermark',
        icon: SvgIconSrc.Watermark,
      });
      sidenavItems.push({
        title: 'Naudotojai',
        link: '/users',
        icon: SvgIconSrc.Users,
      });
    }

    return sidenavItems;
  }
}
