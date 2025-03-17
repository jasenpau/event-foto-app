import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterOutlet, RoutesRecognized } from '@angular/router';
import { LayoutType } from '../components/layouts/layout.types';
import { Subject, takeUntil, tap } from 'rxjs';
import { NgSwitch, NgSwitchCase } from '@angular/common';
import { EmptyLayoutComponent } from '../components/layouts/empty-layout/empty-layout.component';
import { CenterColumnLayoutComponent } from '../components/layouts/center-column-layout/center-column-layout.component';
import { MainLayoutComponent } from '../components/layouts/main-layout/main-layout.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    NgSwitch,
    EmptyLayoutComponent,
    NgSwitchCase,
    CenterColumnLayoutComponent,
    MainLayoutComponent,
  ],
  templateUrl: './root.component.html',
  styleUrl: './root.component.scss',
})
export class RootComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private destroy$: Subject<void> = new Subject<void>();
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
                console.log(routeSnapshot.url[0]);
              }
            }
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
