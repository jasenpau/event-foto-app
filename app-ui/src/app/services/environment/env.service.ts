import { Injectable, isDevMode } from '@angular/core';
import envFile from '../../../environment.json';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';

export type AppEnvironment = typeof envFile;

@Injectable({
  providedIn: 'root',
})
export class EnvService {
  protected config: AppEnvironment = envFile;

  constructor(private readonly http: HttpClient) {}

  loadConfig(): Observable<AppEnvironment> {
    if (isDevMode()) return of(this.config);

    return this.http.get<AppEnvironment>('/environment.json').pipe(
      tap((config) => {
        this.config = config;
      }),
    );
  }

  getConfig(): AppEnvironment {
    return this.config;
  }
}
