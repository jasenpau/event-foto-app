import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  GuardResult,
  MaybeAsync,
  RedirectCommand,
  Router,
} from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { UserService } from '../services/user/user.service';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
  ) {}

  canActivate(route: ActivatedRouteSnapshot): MaybeAsync<GuardResult> {
    const isLoggedIn = this.authService.getUserTokenData() !== null;
    if (!isLoggedIn) {
      const returnUrl = route.url.map((u) => u.path).join('/');
      const loginPath = this.router.parseUrl(`/login?redirectUrl=${returnUrl}`);
      return new RedirectCommand(loginPath, {
        skipLocationChange: true,
      });
    }

    if (this.userService.getCurrentUserData() === null) {
      return this.userService.fetchCurrentUserData().pipe(
        map((data) => {
          return Boolean(data);
        }),
      );
    }

    return true;
  }
}
