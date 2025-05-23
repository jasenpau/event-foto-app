import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoaderService {
  private loadingKeys = new Set<string>();
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public loading$ = this.loadingSubject.asObservable();

  public startLoading(key: string) {
    if (this.loadingKeys.size === 0) {
      this.loadingSubject.next(true);
    }
    this.loadingKeys.add(key);
  }

  public finishLoading(key: string) {
    this.loadingKeys.delete(key);
    if (this.loadingKeys.size === 0) {
      this.loadingSubject.next(false);
    }
  }
}
