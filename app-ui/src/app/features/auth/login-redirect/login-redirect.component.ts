import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { takeUntil, tap } from 'rxjs';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user/user.service';
import { RegisterComponent } from '../register/register.component';
import { NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { handleApiError } from '../../../helpers/handleApiError';
import { LoaderService } from '../../../services/loader/loader.service';

const COMPONENT_LOADING_KEY = 'login-redirect';

@Component({
  selector: 'app-login-redirect',
  imports: [RegisterComponent, NgIf],
  template: '<app-register *ngIf="showRegisterForm" />',
})
export class LoginRedirectComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  showRegisterForm = false;

  private redirectUrl?: string;

  // Inject auth service to handle token redirect
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
    private loaderService: LoaderService,
  ) {
    super();
  }

  ngOnInit() {
    this.loaderService.startLoading(COMPONENT_LOADING_KEY);
    this.authService.tokenEvents
      .pipe(
        tap((event) => {
          if (event.name === 'received') {
            this.redirectUrl = event.state;
            this.initializeUserData();
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
  }

  private initializeUserData() {
    this.userService
      .fetchCurrentUserData()
      .pipe(
        tap((userData) => {
          this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
          if (userData.isActive) {
            this.router.navigate([this.redirectUrl ? this.redirectUrl : '/']);
          } else {
            this.router.navigate(['/register']);
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
