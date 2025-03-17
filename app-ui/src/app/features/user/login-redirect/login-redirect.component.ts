import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { Subject, takeUntil, tap } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-redirect',
  imports: [],
  template: '<p>login-redirect works!</p>',
})
export class LoginRedirectComponent implements OnInit, OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();

  // Inject auth service to handle token redirect
  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
  }

  ngOnInit() {
    this.authService.tokenEvents
      .pipe(
        tap((event) => {
          console.log('token event:', event)
          if (event === 'received') {
            console.log('token received succ');
            this.router.navigate(['/']);
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
    console.log('current user', this.authService.getCurrentUser());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
