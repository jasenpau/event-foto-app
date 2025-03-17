export interface LoginRequestDto {
  email: string,
  password: string,
}

export interface LoginResponseDto {
  token: string,
}

export interface LoginResult {
  success: boolean,
  error?: string,
}

export interface TokenPayload {
  iss: string
  exp: number,
  oid: string,
  name: string
}

export interface User {
  name: string,
  email: string,
  uniqueId: string,
  tokenValidUntil: Date,
}

export enum LoginType {
  EmailPassword = 'email-password',
  MSAL = 'msal'
}
