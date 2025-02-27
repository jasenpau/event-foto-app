import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgForOf } from '@angular/common';

interface SidenavItem {
  title: string;
  link: string;
}

@Component({
  selector: 'app-sidenav',
  imports: [RouterLink, RouterLinkActive, NgForOf],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.scss',
})
export class SidenavComponent {
  sidenavItems: SidenavItem[] = [
    { title: 'Renginiai', link: '/' },
    { title: 'Nustatymai', link: '/settings' },
  ];
}
