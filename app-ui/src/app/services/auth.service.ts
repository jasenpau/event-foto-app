import { Injectable } from '@angular/core';
import { LoginRequestDto, LoginResponseDto, LoginResult, TokenPayload, User } from './auth.types';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ApiBaseUrl } from '../globals/variables';
import { jwtDecode } from 'jwt-decode';

const AUTH_TOKEN_STORAGE_KEY = 'auth-token';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUser: User | null = null;

  constructor(private http: HttpClient) { }

  getCurrentUser() {
    return this.currentUser;
  }

  login(request: LoginRequestDto): Observable<LoginResult> {
    return this.http.post<LoginResponseDto>(`${ApiBaseUrl}/auth/login`, request)
      .pipe(
        map((res: LoginResponseDto) => {
          const decodedToken = this.decodeToken(res.token);
          if (decodedToken) {
            this.setToken(res.token);
            this.currentUser = {
              email: decodedToken.sub,
              id: Number(decodedToken.userId),
              tokenValidUntil: decodedToken.exp
            };
            return { success:true }
          }
          return { success: false }
        }),
        catchError((err: HttpErrorResponse) => {
          if (err.error?.title) {
            const result: LoginResult = { success: false, error: err.error.title };
            return of(result);
          }
          return throwError(() => err);
        })
      );
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      return jwtDecode<TokenPayload>(token);
    } catch (error) {
      console.error('Invalid JWT token', error);
      return null;
    }
  }

  private getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
  }

  private setToken(token: string) {
    localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
  }
}
