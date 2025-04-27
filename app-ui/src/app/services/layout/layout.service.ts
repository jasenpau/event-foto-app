import { Injectable } from '@angular/core';
import { BehaviorSubject, filter, tap } from 'rxjs';
import { NavigationEnd, Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  private readonly showSidenavSubject = new BehaviorSubject(false);

  public showSidenav$ = this.showSidenavSubject.asObservable();

  constructor(private readonly router: Router) {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        tap(() => {
          this.showSidenavSubject.next(false);
        }),
        // takeUntil(this.destroy$),
      )
      .subscribe();
  }

  public toggleSideNav() {
    this.showSidenavSubject.next(!this.showSidenavSubject.value);
  }
}
