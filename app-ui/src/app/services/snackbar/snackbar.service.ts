import { Injectable } from '@angular/core';
import {
  SnackbarAction,
  SnackbarEvent,
  SnackbarMessage,
  SnackbarType,
} from './snackbar.types';
import { Subject } from 'rxjs';

const snackbarTimeout = 5000;

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  public snackbarEvent$ = new Subject<SnackbarEvent>();

  public addSnackbar(type: SnackbarType, message: string, timeout = true) {
    const snackbar: SnackbarMessage = {
      id: Date.now(),
      type,
      message,
    };
    this.snackbarEvent$.next({
      id: snackbar.id,
      action: SnackbarAction.Open,
      message: snackbar,
    });

    if (timeout) {
      setTimeout(() => {
        this.deleteSnackbar(snackbar.id);
      }, snackbarTimeout);
    }

    return snackbar.id;
  }

  public deleteSnackbar(id: number) {
    this.snackbarEvent$.next({
      id,
      action: SnackbarAction.Close,
    });
  }
}
