import { Component } from '@angular/core';
import { ButtonComponent } from '../button/button.component';
import { AuthService } from '../../services/auth/auth.service';
import { User } from '../../services/auth/auth.types';
import { ButtonType } from '../button/button.types';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [ButtonComponent, NgIf],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  currentUser: User | null = null;
  buttonType = ButtonType;

  constructor(private authService: AuthService) {
    this.currentUser = this.authService.getCurrentUser();
  }

  logout() {
    this.authService.logout();
  }
}
