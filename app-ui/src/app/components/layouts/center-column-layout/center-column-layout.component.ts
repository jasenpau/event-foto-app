import { Component, OnDestroy, OnInit } from '@angular/core';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { LoaderService } from '../../../services/loader/loader.service';
import { DisposableComponent } from '../../disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-center-column-layout',
  imports: [SpinnerComponent, NgIf],
  templateUrl: './center-column-layout.component.html',
  styleUrl: './center-column-layout.component.scss',
})
export class CenterColumnLayoutComponent
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
