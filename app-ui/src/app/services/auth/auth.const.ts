import { Configuration } from '@azure/msal-browser';
import { environment } from '../../../environments/environment';

export const AUTH_TOKEN_STORAGE_KEY = 'auth-token';

export const msalConfig: Configuration = {
  auth: {
    clientId: environment.auth.clientId,
    authority: environment.auth.authority,
    redirectUri: `${environment.baseUrl}/redirect`,
    postLogoutRedirectUri: `${environment.baseUrl}/`,
    navigateToLoginRequestUrl: false,
  },
};
