import { Component } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonSize, ButtonType } from '../../../components/button/button.types';
import { AuthService } from '../../../services/auth/auth.service';

@Component({
  selector: 'app-login',
  imports: [ButtonComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  buttonType = ButtonType.Filled;
  buttonSize = ButtonSize.Wide;

  constructor(
    private authService: AuthService,
  ) {}

  async msalLogin() {
    await this.authService.msalLogin();
  }
}
