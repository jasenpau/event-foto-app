import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { takeUntil, tap } from 'rxjs';
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
export class LoginRedirectComponent extends DisposableComponent implements OnInit, OnDestroy {
  showRegisterForm = false;

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
          if (event === 'received') {
            this.triggerUserCheck();
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private triggerUserCheck(): void {
    this.userService.fetchCurrentUserData()
    .pipe(
      tap((exists) => {
        if (exists) {
          this.router.navigate(['/']);
        }
      }),
      handleApiError(error => {
        if (error.title === AppError.UserNotFound) {
          this.showRegisterForm = true;
        }
      }),
      takeUntil(this.destroy$),
    )
    .subscribe();
  }
}
