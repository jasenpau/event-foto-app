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
import { CameraDevice, CameraEvent, CameraSettings } from '../camera.types';
import {
  loadCameraDevice,
  loadCameraEvent,
  saveCameraEvent,
} from '../cameraSettingsHelper';
import { EventService } from '../../../services/event/event.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-camera-main',
  imports: [CameraSetupComponent, NgIf, ButtonComponent],
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
  protected displaySettings =
    this.cameraLoadingFinished &&
    this.eventLoadingFinished &&
    (!this.cameraDevice || !this.event);
  protected userOpenSettings = false;

  protected cameraInitialized = false;
  protected cameraStream?: MediaStream;
  protected uploadInProgress = false;

  constructor(
    private readonly imagingService: ImagingService,
    private readonly eventService: EventService,
  ) {
    super();
  }

  ngOnInit() {
    this.loadCamera();
    this.loadEvent();
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

  capture() {
    this.uploadInProgress = true;
    console.log('CLICK!');
    const ctx = this.canvas.nativeElement.getContext('2d');
    const canvas = this.canvas.nativeElement;
    const video = this.previewVideo.nativeElement;

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    if (ctx) {
      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      canvas.toBlob((blob) => {
        if (blob) {
          this.imagingService.uploadImage(blob).subscribe((response) => {
            console.log('Upload', response);
            setTimeout(() => {
              this.uploadInProgress = false;
            }, 100);
          });
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
    const cameraDevice = loadCameraDevice();
    if (cameraDevice) {
      this.cameraDevice = cameraDevice;
      this.initCamera(cameraDevice.deviceId);
    }
    this.cameraLoadingFinished = true;
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

    this.cameraStream = await navigator.mediaDevices.getUserMedia(constraints);
    if (!this.cameraStream) {
      console.log('failed to init camera', cameraId);
      return;
    }
    this.previewVideo.nativeElement.srcObject = this.cameraStream;
    this.cameraInitialized = true;
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
  }
}
