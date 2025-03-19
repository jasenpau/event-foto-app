import { AppError } from './errors';

export interface ErrorDetails {
  title: AppError | string;
  status: number
}
