import {
  finalize,
  MonoTypeOperatorFunction,
  Observable,
  of,
  switchMap,
  tap,
} from 'rxjs';
import { LoaderService } from '../services/loader/loader.service';

export const useLoader = <T>(
  loadingKey: string,
  loaderService: LoaderService,
): MonoTypeOperatorFunction<T> => {
  return (source$: Observable<any>) =>
    of({}).pipe(
      tap(() => {
        loaderService.startLoading(loadingKey);
      }),
      switchMap(() =>
        source$.pipe(
          finalize(() => {
            loaderService.finishLoading(loadingKey);
          }),
          tap({
            error: () => {
              loaderService.finishLoading(loadingKey);
            },
          }),
        ),
      ),
    );
};

export const useLocalLoader = <T>(
  setLoading: (value: boolean) => void,
): MonoTypeOperatorFunction<T> => {
  return (source$: Observable<any>) =>
    of({}).pipe(
      tap(() => {
        setLoading(true);
      }),
      switchMap(() =>
        source$.pipe(
          finalize(() => {
            setLoading(false);
          }),
        ),
      ),
    );
};
