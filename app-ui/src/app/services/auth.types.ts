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
  userId: string,
  sub: string,
}

export interface User {
  email: string,
  id: number,
  tokenValidUntil: number,
}
