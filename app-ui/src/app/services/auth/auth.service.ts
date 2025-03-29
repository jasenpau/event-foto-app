import { Injectable } from '@angular/core';
import { TokenEvent, TokenPayload, User } from './auth.types';
import { Subject } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import {
  AuthenticationResult,
  PublicClientApplication,
} from '@azure/msal-browser';
import { msalConfig } from './auth.const';

export const AUTH_TOKEN_STORAGE_KEY = 'auth-token';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private currentUser: User | null = null;
  private msalInstance: PublicClientApplication;
  private msalInitialized = false;

  public tokenEvents = new Subject<TokenEvent>();

  constructor() {
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

  getUserTokenData(): User | null {
    if (!this.currentUser) {
      const token = this.getToken();
      if (!token) return null;

      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUser = user;
      }
    }

    // Check if auth token is expired
    if (this.currentUser && new Date() > this.currentUser?.tokenValidUntil) {
      this.currentUser = null;
    }

    return this.currentUser;
  }

  async msalLogin(redirectUrl?: string) {
    if (!this.msalInitialized) return;
    await this.msalInstance.loginRedirect({
      scopes: ['openid', 'profile', `${msalConfig.auth.clientId}/.default`],
      state: redirectUrl,
    });
  }

  msalLogout() {
    this.clearToken();
    this.currentUser = null;
    return this.msalInstance.logoutRedirect();
    // this.router.navigate(['/']);
  }

  private handleMsalLoginRedirect(tokenResponse: AuthenticationResult | null) {
    if (tokenResponse) {
      this.setToken(tokenResponse.accessToken);
      this.currentUser = this.getUserFromToken(tokenResponse.accessToken);
      this.tokenEvents.next({ name: 'received', state: tokenResponse.state });
    }
  }

  private getUserFromToken(token: string): User | null {
    try {
      const decodedToken = jwtDecode<TokenPayload>(token);
      return {
        name: decodedToken.name,
        email: decodedToken.email ?? '',
        uniqueId: decodedToken.oid,
        tokenValidUntil: new Date(decodedToken.exp * 1000),
        groups: decodedToken.groups ?? [],
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
