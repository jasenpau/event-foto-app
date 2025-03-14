import { Injectable } from '@angular/core';
import { CanActivate, GuardResult, MaybeAsync, RedirectCommand, Router } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class CanActivateAuth implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): MaybeAsync<GuardResult> {
    if (this.authService.getCurrentUser()) {
      return true;
    }

    const loginPath = this.router.parseUrl('/login');
    return new RedirectCommand(loginPath, {
      skipLocationChange: true
    })
  }
}
