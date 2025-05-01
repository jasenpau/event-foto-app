export interface CameraDevice {
  deviceId: string;
  label: string;
}

export interface CameraEvent {
  id: number;
  name: string;
}

export interface CameraSettings {
  device?: CameraDevice;
  event?: CameraEvent;
}

export interface UploadMessage {
  eventType: UploadEventType;
  filename: string;
  error?: any;
}

export enum UploadEventType {
  UploadStart = 'upload-start',
  UploadComplete = 'upload-complete',
  UploadError = 'upload-error',
}

export interface UploadData {
  eventId: number;
  filename: string;
  captureDate: Date;
}
