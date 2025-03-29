import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { NgForOf, NgIf } from '@angular/common';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonComponent } from '../../../components/button/button.component';
import { SelectComponent } from '../../../components/forms/select/select.component';
import {
  saveCameraEvent,
  saveCameraDevice,
  requestCameraPermission,
  getVideoDevices,
  loadCameraDevice,
  loadCameraEvent,
} from '../cameraSettingsHelper';
import { EventListDto } from '../../../services/event/event.types';
import { CameraDevice, CameraEvent, CameraSettings } from '../camera.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { EventService } from '../../../services/event/event.service';
import { takeUntil, tap } from 'rxjs';
import { ButtonType } from '../../../components/button/button.types';

@Component({
  selector: 'app-camera-setup',
  imports: [
    NgForOf,
    FormsModule,
    ReactiveFormsModule,
    ButtonComponent,
    SelectComponent,
    NgIf,
  ],
  templateUrl: './camera-setup.component.html',
  styleUrl: './camera-setup.component.scss',
})
export class CameraSetupComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected readonly ButtonType = ButtonType;

  @Output() settingsUpdated = new EventEmitter<CameraSettings>();

  protected cameraDevices: MediaDeviceInfo[] = [];
  protected events: EventListDto[] = [];
  protected settingsForm?: FormGroup;

  constructor(private readonly eventService: EventService) {
    super();
  }

  async ngOnInit() {
    this.initForm();
    this.loadEvents();
    await requestCameraPermission();
    await this.initCameraOptions();
  }

  saveSettings() {
    let settings: CameraSettings = {};

    if (this.settingsForm?.value?.camera) {
      const deviceId = this.settingsForm.value.camera;
      const selectedCamera = this.cameraDevices.find(
        (d) => d.deviceId === deviceId,
      );
      if (!selectedCamera) {
        console.error(`selected device with id '${deviceId}' not found`);
      } else {
        const preferredCamera: CameraDevice = {
          deviceId,
          label: selectedCamera.label,
        };
        saveCameraDevice(preferredCamera);
        settings = { ...settings, device: preferredCamera };
      }
    }

    if (this.settingsForm?.value?.event) {
      const eventId = Number(this.settingsForm.value.event);
      const selectedEvent = this.events.find((e) => e.id === eventId);
      if (!selectedEvent) {
        console.error(`selected event with id '${eventId}' not found`);
      } else {
        const cameraEvent: CameraEvent = {
          id: eventId,
          name: selectedEvent.name,
        };
        saveCameraEvent(cameraEvent);
        settings = { ...settings, event: cameraEvent };
      }
    }

    if (settings.event || settings.device) {
      this.settingsUpdated.emit(settings);
    }
  }

  cancelSettings() {
    this.settingsUpdated.emit({});
  }

  private initForm() {
    const cameraDevice = loadCameraDevice();
    const cameraEvent = loadCameraEvent();

    this.settingsForm = new FormGroup({
      event: new FormControl(cameraEvent?.id ?? null, [Validators.required]),
      camera: new FormControl(cameraDevice?.deviceId ?? null, [
        Validators.required,
      ]),
    });
  }

  private async initCameraOptions() {
    this.cameraDevices = await getVideoDevices();
  }

  private loadEvents() {
    this.eventService
      .getEvents()
      .pipe(
        tap((events) => (this.events = events)),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
