import { throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDetails } from '../globals/types';

export const handleApiError = (err: HttpErrorResponse) => {
  if (err.error?.title) {
    return err.error as ErrorDetails;
  }

  return throwError(() => err);
}
