import { Component, OnDestroy, OnInit } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import {
  ButtonSize,
  ButtonType,
} from '../../../components/button/button.types';
import { AuthService } from '../../../services/auth/auth.service';
import { ActivatedRoute } from '@angular/router';
import { takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';

@Component({
  selector: 'app-login',
  imports: [ButtonComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  buttonType = ButtonType.Filled;
  buttonSize = ButtonSize.Wide;

  private redirectUrl?: string;

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute,
  ) {
    super();
  }

  ngOnInit() {
    this.readRedirectUrlParams();
  }

  async msalLogin() {
    await this.authService.msalLogin(this.redirectUrl);
  }

  private readRedirectUrlParams() {
    this.route.queryParams
      .pipe(
        tap((params) => {
          if (params['redirectUrl']) {
            this.redirectUrl = params['redirectUrl'];
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
