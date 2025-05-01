import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { DisposableComponent } from '../disposable/disposable.component';
import { CameraWorkerService } from '../../services/camera-worker/camera-worker.service';
import { Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-upload-tracker',
  imports: [NgIf],
  templateUrl: './upload-tracker.component.html',
  styleUrl: './upload-tracker.component.scss',
})
export class UploadTrackerComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  private subscriptionDestroy$ = new Subject<void>();
  protected totalUploads = 0;
  protected successfulUploads = 0;
  protected inProgress = false;

  constructor(private readonly cameraWorkerService: CameraWorkerService) {
    super();
  }

  ngOnInit() {
    this.cameraWorkerService.isInitialized$
      .pipe(
        tap((initialized) => {
          if (initialized) this.initListeners();
          else this.subscriptionDestroy$.next();
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private initListeners() {
    this.cameraWorkerService.isUploading$
      .pipe(
        tap((inProgress) => {
          this.inProgress = inProgress;
        }),
        takeUntil(this.subscriptionDestroy$),
      )
      .subscribe();

    this.cameraWorkerService.totalUploads$
      .pipe(
        tap((totalUploads) => {
          this.totalUploads = totalUploads;
        }),
        takeUntil(this.subscriptionDestroy$),
      )
      .subscribe();

    this.cameraWorkerService.successfulUploads$
      .pipe(
        tap((successfulUploads) => {
          this.successfulUploads = successfulUploads;
        }),
        takeUntil(this.subscriptionDestroy$),
      )
      .subscribe();
  }
}
