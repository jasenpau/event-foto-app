import { AUTH_TOKEN_STORAGE_KEY } from '../services/auth/auth.service';

export const getAuthHeaders = () => {
  return {
    headers: {
      Authorization: `Bearer ${localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)}`,
    },
  };
};
