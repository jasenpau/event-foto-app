import { catchError, EMPTY, OperatorFunction, throwError } from 'rxjs';
import { ErrorDetails } from '../globals/types';
import { HttpErrorResponse } from '@angular/common/http';

type ErrorHandler = (err: ErrorDetails) => void;
type HandleApiError = <T>(
  handler: ErrorHandler,
) => OperatorFunction<T, T | never>;

export const handleApiError: HandleApiError = (handler: ErrorHandler) => {
  return catchError((error) => {
    if (
      error instanceof HttpErrorResponse &&
      error.error?.status &&
      (error.error?.title || error.error?.detail)
    ) {
      handler(error.error as ErrorDetails);
      return EMPTY;
    }

    return throwError(() => error);
  });
};
