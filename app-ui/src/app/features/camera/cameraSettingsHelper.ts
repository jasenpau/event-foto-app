import { CameraDevice, CameraEvent } from './camera.types';

const CAMERA_DEVICE_SETTINGS_STORAGE_KEY = 'cameraDeviceSettings';
const CAMERA_EVENT_SETTINGS_STORAGE_KEY = 'cameraEventSettings';

export const saveCameraDevice = (settings: CameraDevice) => {
  localStorage.setItem(
    CAMERA_DEVICE_SETTINGS_STORAGE_KEY,
    JSON.stringify(settings),
  );
};

export const loadCameraDevice = () => {
  const settingsString = localStorage.getItem(
    CAMERA_DEVICE_SETTINGS_STORAGE_KEY,
  );
  if (!settingsString) return null;

  return JSON.parse(settingsString) as CameraDevice;
};

export const saveCameraEvent = (settings: CameraEvent) => {
  localStorage.setItem(
    CAMERA_EVENT_SETTINGS_STORAGE_KEY,
    JSON.stringify(settings),
  );
};

export const loadCameraEvent = () => {
  const settingsString = localStorage.getItem(
    CAMERA_EVENT_SETTINGS_STORAGE_KEY,
  );
  if (!settingsString) return null;

  return JSON.parse(settingsString) as CameraEvent;
};

export const requestCameraPermission = async () => {
  const constraints = { video: true, audio: false };
  const stream = await navigator.mediaDevices.getUserMedia(constraints);
  const tracks = stream.getTracks();
  for (const track of tracks) {
    track.stop();
  }
};

export const getVideoDevices = async () => {
  console.log('gettings cameras...');

  const devices = await navigator.mediaDevices.enumerateDevices();
  const videoDevices = devices.filter((device) => device.kind === 'videoinput');
  console.log('got cameras', videoDevices.length);

  return videoDevices;
};
