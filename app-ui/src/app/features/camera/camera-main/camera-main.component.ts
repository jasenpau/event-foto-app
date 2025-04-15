import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { CameraSetupComponent } from '../camera-setup/camera-setup.component';
import { NgIf } from '@angular/common';
import { ButtonComponent } from '../../../components/button/button.component';
import { ImagingService } from '../../../services/imaging/imaging.service';
import {
  CameraDevice,
  CameraEvent,
  CameraSettings,
  UploadMessage,
} from '../camera.types';
import {
  loadCameraDevice,
  loadCameraEvent,
  saveCameraEvent,
} from '../cameraSettingsHelper';
import { EventService } from '../../../services/event/event.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { AUTH_TOKEN_STORAGE_KEY } from '../../../services/auth/auth.const';
import { UserService } from '../../../services/user/user.service';
import { UploadTrackerComponent } from '../../../components/upload-tracker/upload-tracker.component';

@Component({
  selector: 'app-camera-main',
  imports: [
    CameraSetupComponent,
    NgIf,
    ButtonComponent,
    UploadTrackerComponent,
  ],
  templateUrl: './camera-main.component.html',
  styleUrl: './camera-main.component.scss',
})
export class CameraMainComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  @ViewChild('previewVideo') previewVideo!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  protected cameraDevice?: CameraDevice;
  protected event?: CameraEvent;
  protected cameraLoadingFinished = false;
  protected eventLoadingFinished = false;
  protected userOpenSettings = false;
  protected uploadEvent?: UploadMessage;

  protected cameraInitialized = false;
  protected cameraStream?: MediaStream;

  private cameraWorker: Worker;
  private userId?: string;

  constructor(
    private readonly imagingService: ImagingService,
    private readonly eventService: EventService,
    private readonly userService: UserService,
  ) {
    super();
    this.cameraWorker = new Worker(
      new URL('./camera.worker', import.meta.url),
      {
        type: 'module',
      },
    );
    this.cameraWorker.onmessage = (e) => {
      this.handleWorkerMessage(e);
    };
  }

  ngOnInit() {
    this.loadCamera();
    this.loadEvent();
    this.userId = this.userService.getCurrentUserData()?.id;
  }

  updateSettings(settings: CameraSettings) {
    if (settings.device) {
      this.cameraDevice = settings.device;
      this.cameraLoadingFinished = true;
      this.initCamera(settings.device.deviceId);
    }

    if (settings.event) {
      this.event = settings.event;
      this.eventLoadingFinished = true;
    }

    this.userOpenSettings = false;
  }

  async capture() {
    if (!this.event) return;
    const eventId = this.event.id;

    const ctx = this.canvas.nativeElement.getContext('2d');
    const canvas = this.canvas.nativeElement;
    const video = this.previewVideo.nativeElement;

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    if (ctx) {
      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      canvas.toBlob(async (blob) => {
        if (!blob) return;

        try {
          const filename = `${this.userId}-${Date.now()}.jpg`;

          const root = await navigator.storage.getDirectory();
          const fileHandle = await root.getFileHandle(filename, {
            create: true,
          });
          const writable = await fileHandle.createWritable();

          await writable.write(blob);
          await writable.close();

          const authToken = localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
          if (!authToken) return;

          this.cameraWorker.postMessage({
            eventId,
            filename,
            authToken,
            captureDate: new Date(),
          });
        } catch (err) {
          console.error('Failed to write to OPFS:', err);
        }
      }, 'image/jpeg');
    }
  }

  showSettings() {
    return (
      this.cameraLoadingFinished &&
      this.eventLoadingFinished &&
      (!this.cameraDevice || !this.event)
    );
  }

  openSettings() {
    this.userOpenSettings = true;
  }

  private loadCamera() {
    console.log('loading camera...');
    const cameraDevice = loadCameraDevice();
    if (cameraDevice) {
      this.cameraDevice = cameraDevice;
      this.initCamera(cameraDevice.deviceId).finally(() => {
        this.cameraLoadingFinished = true;
      });
    } else {
      this.cameraLoadingFinished = true;
    }
  }

  private loadEvent() {
    const eventData = loadCameraEvent();
    if (eventData) {
      this.eventService
        .getEventDetails(eventData.id)
        .pipe(
          tap((event) => {
            // Check, if event still exists and refresh the name
            this.event = {
              id: event.id,
              name: event.name,
            };
            saveCameraEvent(eventData);
            this.eventLoadingFinished = true;
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    } else {
      this.eventLoadingFinished = true;
    }
  }

  private async initCamera(cameraId: string) {
    this.stopCamera();
    console.log('loading cameraId', cameraId);

    const constraints = {
      video: {
        deviceId: cameraId ? { exact: cameraId } : undefined,
        width: { ideal: 9999 },
        height: { ideal: 9999 },
      },
    };

    try {
      this.cameraStream =
        await navigator.mediaDevices.getUserMedia(constraints);
      if (!this.cameraStream) {
        this.openSettings();
        return;
      }
      this.previewVideo.nativeElement.srcObject = this.cameraStream;
      this.cameraInitialized = true;
    } catch (error) {
      console.log('failed to init camera', cameraId);
      console.error(error);
      this.openSettings();
    }
  }

  private stopCamera() {
    this.cameraInitialized = false;
    if (this.cameraStream) {
      this.cameraStream.getTracks().forEach((track) => track.stop());
      this.cameraStream = undefined;
      this.previewVideo.nativeElement.srcObject = null;
    }
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    this.stopCamera();
    this.cameraWorker.terminate();
  }

  private handleWorkerMessage(e: MessageEvent) {
    console.log('respone from worker');
    console.log(e.data);
    this.uploadEvent = e.data;
  }
}
