export enum SnackbarType {
  Error = 'error',
  Info = 'info',
  Success = 'success',
}

export enum SnackbarAction {
  Open = 'open',
  Close = 'close',
}

export interface SnackbarMessage {
  id: number;
  type: SnackbarType;
  message: string;
}

export interface SnackbarEvent {
  id: number;
  action: SnackbarAction
  message?: SnackbarMessage
}
