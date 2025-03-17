import { Injectable } from '@angular/core';
import {
  LoginRequestDto,
  LoginResponseDto,
  LoginResult,
  LoginType,
  TokenPayload,
  User,
} from './auth.types';
import { catchError, map, Observable, of, Subject, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ApiBaseUrl } from '../../globals/variables';
import { jwtDecode } from 'jwt-decode';
import { Router } from '@angular/router';
import {
  AuthenticationResult,
  PublicClientApplication,
} from '@azure/msal-browser';
import { msalConfig } from './auth.const';

export const AUTH_TOKEN_STORAGE_KEY = 'auth-token';
export const LOGIN_TYPE_STORAGE_KEY = 'login-type';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private currentUser: User | null = null;
  private msalInstance: PublicClientApplication;
  private msalInitialized = false;
  private loginType: LoginType | null = null;

  public tokenEvents = new Subject<string>();

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {
    this.msalInstance = new PublicClientApplication(msalConfig);
    this.msalInstance.initialize().then(() => {
      this.msalInitialized = true;
      this.msalInstance
        .handleRedirectPromise()
        .then((result) => this.handleMsalLoginRedirect(result))
        .catch((error) => {
          console.log('handleRedirectError', error);
        });
    });
  }

  getCurrentUser() {
    if (!this.currentUser) {
      console.log('current user is null, trying token');
      const token = this.getToken();
      console.log('stored token', token);
      if (!token) return null;

      const user = this.getUserFromToken(token);
      console.log('stored user fromt token', user);
      if (user) {
        console.log('setting user');
        this.currentUser = user;
      }
    }

    // Check if auth token is expired
    if (this.currentUser && new Date() > this.currentUser?.tokenValidUntil) {
      console.log('token expired :/');
      this.currentUser = null;
    }

    return this.currentUser;
  }

  login(request: LoginRequestDto): Observable<LoginResult> {
    return this.http
      .post<LoginResponseDto>(`${ApiBaseUrl}/auth/login`, request)
      .pipe(
        map((res: LoginResponseDto) => {
          const user = this.getUserFromToken(res.token);
          if (user) {
            this.setToken(res.token, LoginType.EmailPassword);
            this.loginType = LoginType.EmailPassword;
            this.currentUser = user;
            return { success: true };
          }
          return { success: false };
        }),
        catchError((err: HttpErrorResponse) => {
          if (err.error?.title) {
            const result: LoginResult = {
              success: false,
              error: err.error.title,
            };
            return of(result);
          }
          return throwError(() => err);
        }),
      );
  }

  async msalLogin() {
    if (!this.msalInitialized) return;
    await this.msalInstance.loginRedirect({
      scopes: ['openid', 'profile', `${msalConfig.auth.clientId}/.default`],
      prompt: 'consent'
    });
  }

  logout() {
    this.clearToken();
    this.currentUser = null;

    if (this.loginType === LoginType.MSAL) {
      this.loginType = null;
      return this.msalInstance.logoutRedirect();
    } else {
      this.loginType = null;
      return this.router.navigate(['/login']);
    }
  }

  private handleMsalLoginRedirect(tokenResponse: AuthenticationResult | null) {
    if (tokenResponse) {
      this.setToken(tokenResponse.accessToken, LoginType.MSAL);
      this.currentUser = this.getUserFromToken(tokenResponse.accessToken);
      this.tokenEvents.next('received');
    }
    console.log('token', tokenResponse);
  }

  private getUserFromToken(token: string): User | null {
    try {
      const decodedToken = jwtDecode<TokenPayload>(token);
      return {
        name: decodedToken.name,
        email: 'PLACEHOLDER',
        uniqueId: decodedToken.oid,
        tokenValidUntil: new Date(decodedToken.exp * 1000),
      };
    } catch (error) {
      console.error('Invalid JWT token', error);
      return null;
    }
  }

  private getToken(): string | null {
    const token = localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
    if (!token) return null;

    this.loginType = localStorage.getItem(LOGIN_TYPE_STORAGE_KEY) as LoginType;
    return token;
  }

  private setToken(token: string, loginType: LoginType) {
    this.loginType = loginType;
    localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
    localStorage.setItem(LOGIN_TYPE_STORAGE_KEY, this.loginType);
  }

  private clearToken() {
    localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
    localStorage.removeItem(LOGIN_TYPE_STORAGE_KEY);
  }
}
