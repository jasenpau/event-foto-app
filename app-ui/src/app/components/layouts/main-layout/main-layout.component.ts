import { Component, OnDestroy, OnInit } from '@angular/core';
import { SidenavComponent } from '../../sidenav/sidenav.component';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { DisposableComponent } from '../../disposable/disposable.component';
import { LoaderService } from '../../../services/loader/loader.service';
import { takeUntil, tap } from 'rxjs';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-main-layout',
  imports: [SidenavComponent, SpinnerComponent, NgIf],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected showLoader = false;

  constructor(private readonly loaderService: LoaderService) {
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
  }
}
