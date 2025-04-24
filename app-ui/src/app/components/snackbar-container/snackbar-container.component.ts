import { Component, OnDestroy, OnInit } from '@angular/core';
import { SvgIconSrc } from '../svg-icon/svg-icon.types';
import { AppSvgIconComponent } from '../svg-icon/app-svg-icon.component';
import { SnackbarService } from '../../services/snackbar/snackbar.service';
import { DisposableComponent } from '../disposable/disposable.component';
import { tap } from 'rxjs';
import {
  SnackbarAction,
  SnackbarMessage,
  SnackbarType,
} from '../../services/snackbar/snackbar.types';
import { NgClass, NgForOf, NgIf } from '@angular/common';
import { IconButtonComponent } from '../icon-button/icon-button.component';

@Component({
  selector: 'app-snackbar-container',
  imports: [AppSvgIconComponent, NgForOf, NgClass, IconButtonComponent, NgIf],
  templateUrl: './snackbar-container.component.html',
  styleUrl: './snackbar-container.component.scss',
})
export class SnackbarContainerComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  svgIconSrc = SvgIconSrc;
  protected snackbarMessages: SnackbarMessage[] = [];

  constructor(private snackbarService: SnackbarService) {
    super();
  }

  ngOnInit() {
    this.snackbarService.snackbarEvent$
      .pipe(
        tap((event) => {
          if (event.action === SnackbarAction.Open && event.message) {
            this.snackbarMessages.push(event.message);
          } else if (event.action === SnackbarAction.Close) {
            this.closeSnackbar(event.id);
          }
        }),
      )
      .subscribe();
  }

  getIcon(type: SnackbarType) {
    switch (type) {
      case SnackbarType.Error:
        return SvgIconSrc.ErrorCircle;
      case SnackbarType.Info:
        return SvgIconSrc.InfoCircle;
      case SnackbarType.Success:
        return SvgIconSrc.CheckCircle;
      case SnackbarType.Loading:
        return SvgIconSrc.Downloading;
    }
  }

  closeSnackbar(id: number) {
    this.snackbarMessages = this.snackbarMessages.filter(
      (message) => message.id !== id,
    );
  }

  protected readonly SnackbarType = SnackbarType;
}
