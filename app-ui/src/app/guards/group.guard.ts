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
import { UserGroup } from '../globals/userGroups';

@Injectable({
  providedIn: 'root',
})
export class GroupPermissionGuard implements CanActivate {
  constructor(
    private userService: UserService,
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

    const canActivate = this.userService
      .getUserGroups()
      .includes(requiredGroup);
    if (canActivate) return true;

    const noAccessRoute = this.router.parseUrl('/no-access');
    return new RedirectCommand(noAccessRoute, {
      skipLocationChange: true,
    });
  }
}
