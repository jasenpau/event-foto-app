import { Configuration } from '@azure/msal-browser';
import { AppEnvironment } from '../environment/env.service';

export const AUTH_TOKEN_STORAGE_KEY = 'auth-token';

export const buildMsalConfig = (env: AppEnvironment): Configuration => ({
  auth: {
    clientId: env.auth.clientId,
    authority: env.auth.authority,
    redirectUri: `${env.baseUrl}/redirect`,
    postLogoutRedirectUri: `${env.baseUrl}/`,
    navigateToLoginRequestUrl: false,
  },
});
