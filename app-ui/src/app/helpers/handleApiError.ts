import { catchError, of, throwError } from 'rxjs';
import { ErrorDetails } from '../globals/types';
import { HttpErrorResponse } from '@angular/common/http';

type ErrorHandler = (err: ErrorDetails) => void;

export const handleApiError = (handler: ErrorHandler) => {
  return catchError((error) => {
    if (error instanceof HttpErrorResponse && error.error?.status && error.error?.title) {
      handler(error.error as ErrorDetails);
      return of(null);
    }

    return throwError(() => error);
  });
}
