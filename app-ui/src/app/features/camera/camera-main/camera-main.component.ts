import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CameraSetupComponent } from '../camera-setup/camera-setup.component';
import { NgIf } from '@angular/common';
import { ButtonComponent } from '../../../components/button/button.component';
import { ImagingService } from '../../../services/imaging/imaging.service';
import { tap } from 'rxjs';

@Component({
  selector: 'app-camera-main',
  imports: [CameraSetupComponent, NgIf, ButtonComponent],
  templateUrl: './camera-main.component.html',
  styleUrl: './camera-main.component.scss',
})
export class CameraMainComponent implements OnDestroy {
  @ViewChild('previewVideo') previewVideo!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  protected cameraId?: string;
  protected cameraInitialized = false;
  protected cameraStream?: MediaStream;
  protected uploadInProgress = false;

  constructor(private readonly imagingService: ImagingService) {
  }

  setCameraId(cameraId: string) {
    this.cameraId = cameraId;
    this.initCamera(cameraId);
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
          this.imagingService.uploadImage(blob)
            .subscribe(response => {
              console.log('Upload', response);
              setTimeout(() => {
                this.uploadInProgress = false;
              }, 100);
            })
        }
      }, 'image/jpeg');
    }
  }

  private async initCamera(cameraId: string) {
    this.stopCamera();

    const constraints = {
      video: {
        deviceId: cameraId ? { exact: cameraId } : undefined,
        width: { ideal: 9999 },
        height: { ideal: 9999 },
      },
    };

    this.cameraStream = await navigator.mediaDevices.getUserMedia(constraints);
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

  ngOnDestroy() {
    this.stopCamera();
  }
}
