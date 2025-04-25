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
import { UserData } from '../services/user/user.types';

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

    const userData = this.userService.getCurrentUserData();
    if (userData) return this.userActiveCheck(userData, route);

    return this.userService
      .fetchCurrentUserData()
      .pipe(map((user) => this.userActiveCheck(user, route)));
  }

  private userActiveCheck = (user: UserData, route: ActivatedRouteSnapshot) => {
    if (route.routeConfig?.data?.['ignoreActiveCheck'] === true) return true;

    const registerPath = this.router.parseUrl('/register');
    return user.isActive ? true : new RedirectCommand(registerPath);
  };
}
