import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  GuardResult,
  MaybeAsync,
  RedirectCommand,
  Router,
} from '@angular/router';
import { UserService } from '../services/user/user.service';
import { map } from 'rxjs';
import { AuthService } from '../services/auth/auth.service';
import { UserGroup } from '../globals/userGroups';

@Injectable({
  providedIn: 'root',
})
export class GroupPermissionGuard implements CanActivate {
  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
  ) {}

  canActivate(route: ActivatedRouteSnapshot): MaybeAsync<GuardResult> {
    const requiredGroup = route.data['requiredGroup'] as UserGroup;
    if (!requiredGroup) {
      console.warn(
        'No requiredGroup with GroupPermissionGuard for route',
        route.title,
      );
    }

    return this.userService.userGroups().pipe(
      map((groups) => {
        const canActivate = groups.includes(requiredGroup);
        if (canActivate) return true;

        const noAccessRoute = this.router.parseUrl('/no-access');
        return new RedirectCommand(noAccessRoute, {
          skipLocationChange: true,
        });
      }),
    );
  }
}
