import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { takeUntil, tap } from 'rxjs';
import { LoaderService } from '../../../services/loader/loader.service';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth/auth.service';

const COMPONENT_LOADING_KEY = 'invite-entry';

@Component({
  selector: 'app-invite-entry',
  imports: [NgIf],
  templateUrl: './invite-entry.component.html',
  styleUrl: './invite-entry.component.scss',
})
export class InviteEntryComponent
  extends DisposableComponent
  implements OnDestroy
{
  protected showInvalidNotice = false;

  constructor(
    private readonly userService: UserService,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly loaderService: LoaderService,
    private readonly router: Router,
  ) {
    super();
    this.loaderService.startLoading(COMPONENT_LOADING_KEY);
    this.readRouteParams();
  }

  private readRouteParams() {
    this.route.paramMap.subscribe((params) => {
      const inviteKey = params.get('inviteKey');
      if (inviteKey) {
        this.validateInvite(inviteKey);
      } else {
        this.router.navigate(['/']);
      }
    });
  }

  private validateInvite(inviteKey: string) {
    this.userService
      .validateInvite(inviteKey)
      .pipe(
        tap((valid) => {
          this.loaderService.finishLoading(COMPONENT_LOADING_KEY);
          if (valid) {
            this.authService.msalLogin();
          } else {
            this.showInvalidNotice = true;
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
