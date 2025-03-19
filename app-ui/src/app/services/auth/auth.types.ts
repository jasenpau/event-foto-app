export interface TokenPayload {
  iss: string
  exp: number,
  oid: string,
  name: string,
  email?: string,
}

export interface User {
  name: string,
  email: string,
  uniqueId: string,
  tokenValidUntil: Date,
}
