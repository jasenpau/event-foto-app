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
