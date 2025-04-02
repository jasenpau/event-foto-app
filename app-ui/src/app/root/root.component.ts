import {
  AfterViewInit,
  Component,
  inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { Router, RouterOutlet, RoutesRecognized } from '@angular/router';
import { LayoutType } from '../components/layouts/layout.types';
import { takeUntil, tap } from 'rxjs';
import { NgSwitch, NgSwitchCase } from '@angular/common';
import { EmptyLayoutComponent } from '../components/layouts/empty-layout/empty-layout.component';
import { CenterColumnLayoutComponent } from '../components/layouts/center-column-layout/center-column-layout.component';
import { MainLayoutComponent } from '../components/layouts/main-layout/main-layout.component';
import { DisposableComponent } from '../components/disposable/disposable.component';
import { Lithuanian } from 'flatpickr/dist/l10n/lt';
import flatpickr from 'flatpickr';
import { SnackbarContainerComponent } from '../components/snackbar-container/snackbar-container.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    NgSwitch,
    EmptyLayoutComponent,
    NgSwitchCase,
    CenterColumnLayoutComponent,
    MainLayoutComponent,
    SnackbarContainerComponent,
  ],
  templateUrl: './root.component.html',
  styleUrl: './root.component.scss',
})
export class RootComponent
  extends DisposableComponent
  implements OnInit, OnDestroy, AfterViewInit
{
  private readonly router = inject(Router);
  protected readonly layoutType = LayoutType;

  currentLayout: LayoutType | null = null;

  ngOnInit() {
    this.router.events
      .pipe(
        tap((event) => {
          if (event instanceof RoutesRecognized) {
            const routeSnapshot = event.state.root.firstChild;
            if (routeSnapshot) {
              const layout = routeSnapshot.data['layout'];
              if (layout) this.currentLayout = layout;
              else {
                this.currentLayout = LayoutType.Empty;
                console.warn(
                  'No layout specified for route',
                  routeSnapshot.title,
                );
              }
            }
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  ngAfterViewInit() {
    flatpickr.localize(Lithuanian);
  }
}
