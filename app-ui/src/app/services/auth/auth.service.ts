import { Injectable } from '@angular/core';
import { LoginRequestDto, LoginResponseDto, LoginResult, TokenPayload, User } from './auth.types';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ApiBaseUrl } from '../../globals/variables';
import { jwtDecode } from 'jwt-decode';
import { Router } from '@angular/router';

export const AUTH_TOKEN_STORAGE_KEY = 'auth-token';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUser: User | null = null;

  constructor(private http: HttpClient, private router: Router) { }

  getCurrentUser() {
    if (!this.currentUser) {
      const token = this.getToken();
      if (!token) return null;

      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUser = user;
      }
    }

    // Check if auth token is expired
    if (this.currentUser && Date.now() > this.currentUser?.tokenValidUntil * 1000)  {
      this.currentUser = null;
    }

    return this.currentUser;
  }

  login(request: LoginRequestDto): Observable<LoginResult> {
    return this.http.post<LoginResponseDto>(`${ApiBaseUrl}/auth/login`, request)
      .pipe(
        map((res: LoginResponseDto) => {
          const user = this.getUserFromToken(res.token);
          if (user) {
            this.setToken(res.token);
            this.currentUser = user;
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

  logout() {
    this.clearToken();
    this.currentUser = null;
    return this.router.navigate(['/login']);
  }

  private getUserFromToken(token: string): User | null {
    try {
      const decodedToken = jwtDecode<TokenPayload>(token);
      return {
        email: decodedToken.sub,
        id: Number(decodedToken.userId),
        tokenValidUntil: decodedToken.exp
      };
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

  private clearToken() {
    localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
  }
}
