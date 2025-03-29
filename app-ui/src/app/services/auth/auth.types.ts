export interface TokenPayload {
  iss: string;
  exp: number;
  oid: string;
  name: string;
  email?: string;
  groups?: string[];
}

export interface User {
  name: string;
  email: string;
  uniqueId: string;
  tokenValidUntil: Date;
  groups: string[];
}

export interface TokenEvent {
  name: string;
  state?: string;
}
