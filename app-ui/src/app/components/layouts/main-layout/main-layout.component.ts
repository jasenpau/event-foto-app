import { Component, OnDestroy, OnInit } from '@angular/core';
import { SidenavComponent } from '../../sidenav/sidenav.component';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { DisposableComponent } from '../../disposable/disposable.component';
import { LoaderService } from '../../../services/loader/loader.service';
import { takeUntil, tap } from 'rxjs';
import { NgClass, NgIf } from '@angular/common';
import { IconButtonComponent } from '../../icon-button/icon-button.component';
import { SvgIconSrc } from '../../svg-icon/svg-icon.types';
import { LayoutService } from '../../../services/layout/layout.service';

@Component({
  selector: 'app-main-layout',
  imports: [
    SidenavComponent,
    SpinnerComponent,
    NgIf,
    IconButtonComponent,
    NgClass,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected showLoader = false;
  protected showSidenav = false;

  constructor(
    private readonly loaderService: LoaderService,
    private readonly layoutService: LayoutService,
  ) {
    super();
  }

  ngOnInit() {
    this.loaderService.loading$
      .pipe(
        tap((isLoading) => {
          this.showLoader = isLoading;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();

    this.layoutService.showSidenav$
      .pipe(
        tap((value) => (this.showSidenav = value)),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  protected readonly SvgIconSrc = SvgIconSrc;

  toggleSidenav() {
    this.layoutService.toggleSideNav();
  }
}
