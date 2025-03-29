import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { firstValueFrom, takeUntil, tap } from 'rxjs';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user/user.service';
import { RegisterComponent } from '../register/register.component';
import { NgIf } from '@angular/common';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { handleApiError } from '../../../helpers/handleApiError';
import { AppError } from '../../../globals/errors';

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
  ) {
    super();
  }

  ngOnInit() {
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

  private initializeUserData() {
    this.userService
      .fetchCurrentUserData()
      .pipe(
        tap(async (exists) => {
          await this.loadAppData();
          if (exists) {
            await this.router.navigate([
              this.redirectUrl ? this.redirectUrl : '/',
            ]);
          }
        }),
        handleApiError((error) => {
          if (error.title === AppError.UserNotFound) {
            this.showRegisterForm = true;
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private async loadAppData() {
    await firstValueFrom(
      this.userService.getAppGroups().pipe(takeUntil(this.destroy$)),
    );
  }
}
