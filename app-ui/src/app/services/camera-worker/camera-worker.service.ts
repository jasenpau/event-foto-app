import { Injectable } from '@angular/core';
import {
  UploadData,
  UploadEventType,
  UploadMessage,
} from '../../features/camera/camera.types';
import { AUTH_TOKEN_STORAGE_KEY } from '../auth/auth.const';
import { EnvService } from '../environment/env.service';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CameraWorkerService {
  private readonly apiBaseUrl: string;
  private cameraWorker?: Worker;
  private activeUploads = new Set<string>();
  private erroredUploads = new Set<string>();
  private totalUploads = 0;
  private successfulUploads = 0;
  private terminateRequested = false;

  private totalUploadsSubject = new BehaviorSubject<number>(0);
  private successfulUploadsSubject = new BehaviorSubject<number>(0);
  private isUploadingSubject = new BehaviorSubject<boolean>(false);
  private isInitializedSubject = new BehaviorSubject<boolean>(false);

  public get totalUploads$() {
    return this.totalUploadsSubject.asObservable();
  }

  public get successfulUploads$() {
    return this.successfulUploadsSubject.asObservable();
  }

  public get isUploading$() {
    return this.isUploadingSubject.asObservable();
  }

  public get isInitialized$() {
    return this.isInitializedSubject.asObservable();
  }

  constructor(private readonly envService: EnvService) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

  public initialize(): void {
    if (this.cameraWorker) return;

    this.cameraWorker = new Worker(
      new URL('./camera.worker', import.meta.url),
      {
        type: 'module',
      },
    );

    this.cameraWorker.onmessage = (e) => {
      this.handleWorkerMessage(e);
    };

    this.isInitializedSubject.next(true);
  }

  public requestTerminate() {
    if (this.cameraWorker) {
      this.terminateRequested = true;
      this.terminateIfDone();
    }
  }

  public postMessage(uploadData: UploadData) {
    if (this.cameraWorker) {
      const authToken = localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
      if (!authToken) return;

      this.cameraWorker.postMessage({
        eventId: uploadData.eventId,
        filename: uploadData.filename,
        authToken,
        apiBaseUrl: this.apiBaseUrl,
        captureDate: uploadData.captureDate,
      });
    }
  }

  private handleWorkerMessage(e: MessageEvent) {
    this.uploadEvent(e.data);
    this.terminateIfDone();
  }

  private uploadEvent(message: UploadMessage | undefined) {
    if (!message) return;

    switch (message.eventType) {
      case UploadEventType.UploadStart:
        this.activeUploads.add(message.filename);
        this.totalUploads += 1;
        this.totalUploadsSubject.next(this.totalUploads);
        this.isUploadingSubject.next(true);
        break;
      case UploadEventType.UploadComplete:
        this.activeUploads.delete(message.filename);
        this.successfulUploads += 1;
        this.successfulUploadsSubject.next(this.successfulUploads);
        this.isUploadingSubject.next(this.activeUploads.size > 0);
        break;
      case UploadEventType.UploadError:
        this.activeUploads.delete(message.filename);
        this.erroredUploads.add(message.filename);
        this.isUploadingSubject.next(this.activeUploads.size > 0);
        break;
    }
  }

  private terminateIfDone() {
    if (
      this.cameraWorker &&
      this.terminateRequested &&
      this.activeUploads.size === 0
    ) {
      this.isInitializedSubject.next(false);
      this.cameraWorker.terminate();
      this.cameraWorker = undefined;
      this.terminateRequested = false;

      this.successfulUploadsSubject.complete();
      this.totalUploadsSubject.complete();
      this.isUploadingSubject.complete();

      this.erroredUploads.clear();
      this.totalUploads = 0;
      this.successfulUploads = 0;

      this.totalUploadsSubject = new BehaviorSubject<number>(0);
      this.successfulUploadsSubject = new BehaviorSubject<number>(0);
      this.isUploadingSubject = new BehaviorSubject<boolean>(false);
    }
  }
}
